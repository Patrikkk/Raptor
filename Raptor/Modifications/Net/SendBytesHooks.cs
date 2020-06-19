using Mono.Cecil;
using Mono.Cecil.Cil;
using Raptor.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;

namespace Raptor.Modifications.Net
{
	using static Instruction;

	internal sealed class SendBytesHooks : Modification
	{
		private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

		public override void Apply(AssemblyDefinition assembly)
		{
			var netMessage = assembly.GetType("NetMessage");
			var module = netMessage.Module;
			var sendData = netMessage.GetMethod("SendData");

			var instructions = sendData.Body.Instructions.Where(x => x.OpCode == OpCodes.Callvirt
																	&& x.Operand is MethodReference
																	&& (x.Operand as MethodReference).Name == "AsyncSend" 
																	&& x.Previous.Previous.Previous.Previous.Operand is FieldReference
																	&& (x.Previous.Previous.Previous.Previous.Operand as FieldReference).Name == "Connection"
																	);

			sendData.InjectBefore(
				instructions.First(),
				Create(OpCodes.Ldsfld, netMessage.Fields.First(f => f.Name == "buffer")),
				Create(OpCodes.Ldloc_0),
				Create(OpCodes.Ldelem_Ref),
				Create(OpCodes.Ldfld, assembly.GetType("MessageBuffer").Fields.First(f => f.Name == "writeBuffer")),
				Create(OpCodes.Ldc_I4_0),
				Create(OpCodes.Ldloc_S, sendData.Body.Variables.ElementAt(5)),
				Create(OpCodes.Call, module.Import(typeof(NetHooks).GetMethod("InvokeSendBytes", Flags)))
				);
		}
	}
}
