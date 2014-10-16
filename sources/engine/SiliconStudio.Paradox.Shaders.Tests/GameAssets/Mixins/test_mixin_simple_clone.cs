// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Paradox Shader Mixin Code Generator.
// To generate it yourself, please install SiliconStudio.Paradox.VisualStudio.Package .vsix
// and re-save the associated .pdxfx.
// </auto-generated>

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;


#line 1 "C:\Projects\Paradox\sources\engine\SiliconStudio.Paradox.Shaders.Tests\GameAssets\Mixins\test_mixin_simple_clone.pdxfx"
namespace Test
{

    #line 3
    public partial class ChildClone  : IShaderMixinBuilder
    {
        public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
        {

            #line 5
            mixin.Mixin.CloneFrom(mixin.Parent.Mixin);

            #line 6
            context.Mixin(mixin, "C1");

            #line 7
            context.Mixin(mixin, "C2");
        }

        [ModuleInitializer]
        internal static void __Initialize__()

        {
            ShaderMixinManager.Register("ChildClone", new ChildClone());
        }
    }

    #line 10
    public partial class DefaultSimpleClone  : IShaderMixinBuilder
    {
        public void Generate(ShaderMixinSourceTree mixin, ShaderMixinContext context)
        {

            #line 12
            context.Mixin(mixin, "A");

            #line 13
            context.Mixin(mixin, "B");

            #line 14
            context.Mixin(mixin, "C");

            {

                #line 15
                var __subMixin = new ShaderMixinSourceTree() { Parent = mixin };
                mixin.Children.Add(__subMixin);

                #line 15
                context.Mixin(__subMixin, "ChildClone");
            }
        }

        [ModuleInitializer]
        internal static void __Initialize__()

        {
            ShaderMixinManager.Register("DefaultSimpleClone", new DefaultSimpleClone());
        }
    }
}
