﻿module FSharpMvc.Combinators

open System
open System.Web.Mvc
open System.Globalization
open System.Collections.Specialized
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

let contentAction (action: unit -> string) =
    fun (ctx: ControllerContext) -> action() |> Result.content

let formAction (action: NameValueCollection -> ActionResult) =
    fun (ctx: ControllerContext) -> action ctx.HttpContext.Request.Form

let bindForm (action: 'a -> 'b) (e: Expr<'a -> 'b>) = 
    let pname = match e with
                | Lambda(var, body) -> var.Name
                | x -> failwithf "Expected lambda, actual %A" x

    let binder = ModelBinders.Binders.GetBinder typeof<'a>
    fun (ctx: ControllerContext) ->
        let bindingContext = ModelBindingContext(
                                ModelName = pname, 
                                ModelState = ctx.Controller.ViewData.ModelState, 
                                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof<'a>),
                                ValueProvider = ctx.Controller.ValueProvider)
        let r = binder.BindModel(ctx, bindingContext)
        if not bindingContext.ModelState.IsValid
            then failwith "Binding failed"
        action (r :?> 'a)

let bindFormToRecord (action: 'a -> ActionResult) =
    ()