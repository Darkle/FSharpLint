module FSharpLint.Rules.UsedUnderscorePrefixedElements

open System

open FSharpLint.Framework
open FSharpLint.Framework.Suggestion
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text
open FSharp.Compiler.CodeAnalysis
open FSharpLint.Framework.Ast
open FSharpLint.Framework.Rules

let private checkUsedIdent (identifier: Ident) (usages: array<FSharpSymbolUse>) (scopeRange: Range) =
    usages
    |> Array.collect (fun usage ->
        if not usage.IsFromDefinition && usage.Symbol.FullName = identifier.idText 
            && ExpressionUtilities.rangeContainsOtherRange scopeRange usage.Range then
            {
                Range = usage.Range
                Message = String.Format(Resources.GetString ("RulesUsedUnderscorePrefixedElements"))
                SuggestedFix = None
                TypeChecks = List.Empty
            } |> Array.singleton
        else
            Array.empty)

let runner (args: AstNodeRuleParams) =
    // hack to only run rule once
    if args.NodeIndex = 0 then
        match args.CheckInfo with
        | Some checkResults -> 
            checkResults.GetAllUsesOfAllSymbolsInFile() 
            |> Seq.choose (fun usage ->
                if not usage.IsFromDefinition && usage.Symbol.FullName.StartsWith "_" then
                    Some {
                        Range = usage.Range
                        Message = String.Format(Resources.GetString ("RulesUsedUnderscorePrefixedElements"))
                        SuggestedFix = None
                        TypeChecks = List.Empty
                    }
                else
                    None)
            |> Seq.toArray
        | None -> Array.empty
    else
        Array.empty

let rule =
    { Name = "UsedUnderscorePrefixedElements"
      Identifier = Identifiers.UsedUnderscorePrefixedElements
      RuleConfig = { AstNodeRuleConfig.Runner = runner; Cleanup = ignore } }
    |> AstNodeRule
