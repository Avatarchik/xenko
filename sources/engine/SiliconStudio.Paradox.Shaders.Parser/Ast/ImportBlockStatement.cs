// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.Shaders.Parser.Ast
{
    public class ImportBlockStatement : BlockStatement
    {
        public string Name { get; set; }
    }
}