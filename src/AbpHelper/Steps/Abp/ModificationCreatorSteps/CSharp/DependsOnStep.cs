﻿using System.Collections.Generic;
using System.Linq;
using EasyAbp.AbpHelper.Extensions;
using EasyAbp.AbpHelper.Generator;
using EasyAbp.AbpHelper.Models;
using EasyAbp.AbpHelper.Steps.Common;
using EasyAbp.AbpHelper.Workflow;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services.Models;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps.CSharp
{
    public class DependsOnStep : CSharpModificationCreatorStep
    {
        public DependsOnStep([NotNull] TextGenerator textGenerator) : base(textGenerator)
        {
        }

        protected override IList<ModificationBuilder<CSharpSyntaxNode>> CreateModifications(WorkflowExecutionContext context, CompilationUnitSyntax rootUnit)
        {
            var moduleClassNamePostfix = context.GetVariable<string>(VariableNames.ModuleClassNamePostfix);
            var dependsOnClassName = context.GetVariable<string>(VariableNames.DependsOnModuleClassName);
            string templateDir = context.GetVariable<string>(VariableNames.TemplateDirectory);
            var model = context.GetVariable<dynamic>("Model");
            model.Bag.ModuleClassNamePostfix = moduleClassNamePostfix;
            model.Bag.DependsOnModuleClassName = dependsOnClassName;
            string usingText = TextGenerator.GenerateByTemplateName(templateDir, "ModuleClass_Using", model);
            string dependsOnText = TextGenerator.GenerateByTemplateName(templateDir, "ModuleClass_DependsOn", model);

            return new List<ModificationBuilder<CSharpSyntaxNode>>
            {
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<UsingDirectiveSyntax>().Last().GetEndLine(),
                    insertPosition: InsertPosition.After,
                    contents: usingText,
                    modifyCondition: root => root.DescendantsNotContain<UsingDirectiveSyntax>(usingText)
                ),
                new InsertionBuilder<CSharpSyntaxNode>(
                    root => root.Descendants<ClassDeclarationSyntax>().First().Keyword.GetStartLine(),
                    dependsOnText,
                    modifyCondition: root => root.DescendantsNotContain<ClassDeclarationSyntax>(dependsOnText)
                ),
            };
        }
    }
}