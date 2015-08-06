System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

#r @"packages\FSharp.Data.TypeProviders.0.0.1\lib\net40\FSharp.Data.TypeProviders.dll"
#r @"System.Data.Linq.dll"

open System
open System.Linq
open System.Data
open Microsoft.FSharp.Data.TypeProviders
open System.Text.RegularExpressions

[<Literal>]
let connectionString = "Data Source=(local)\sqlserver2014; Initial Catalog=SqlInFsharp; Integrated Security=true;"

type Sql = SqlDataConnection<connectionString>
let db = Sql.GetDataContext()
db.DataContext.Log <- Console.Out

let getDomain email =
    Regex.Match(email,".*@(.*)").Groups.[1].Value

let getDecade (birthdate:Nullable<DateTime>) =
    if birthdate.HasValue then 
        birthdate.Value.Year / 10 * 10 |> Some
    else
        None

let topDomain list =
    list
    |> Seq.distinct
    |> Seq.head
    |> snd

db.Customer
|> Seq.map (fun c -> getDecade c.Birthdate, getDomain c.Email)
|> Seq.groupBy fst
|> Seq.sortBy fst
|> Seq.map (fun (decade, group) -> (decade,topDomain group))
|> Seq.iter (printfn "%A")
