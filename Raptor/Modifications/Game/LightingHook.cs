using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Raptor.Hooks;

namespace Raptor.Modifications.Game
{
    using static Instruction;
#if DEBUG
    internal sealed class LightingHook : Modification
    {
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Static;

        private static void InjectHooks(TypeDefinition type)
        {
            var method = type.GetMethod("UpdateGlobalBrightness");
            var module = method.Module;
            var body = method.Body;
            var instructions = body.Instructions;
            for (var i = instructions.Count - 1; i >= 0; --i)
            {
                // Check for an invocation of some action involving LightingSwipeData.
                var instruction = instructions[i];
                if (instruction.OpCode != OpCodes.Ret)
                {
                    continue;
                }

                // The function field of LightingSwipeData has already been loaded. Thus, we need to move back two
                // instructions if we want our hooks to be able to modify function.
                var target = instruction.Next;
                method.InjectEndings( 
                    Create(OpCodes.Call, module.Import(typeof(GameHooks).GetMethod("InvokeLighting", Flags)))
                    );
                i -= 5;
            }
        }

        public override void Apply(AssemblyDefinition assembly)
        {
/*
            var lighting = assembly.GetType("Lighting");

            // Single-core lighting
            var doColors = lighting.GetMethod("UpdateGlobalBrightness");
            InjectHooks(lighting);
            doColors.ReplaceShortBranches();*/

            // Multi-core lighting
           /* var callbackLightingSwipe = lighting.GetMethod("callback_LightingSwipe");
            InjectHooks(callbackLightingSwipe);
            callbackLightingSwipe.ReplaceShortBranches();*/

        }
    }
#endif
}
