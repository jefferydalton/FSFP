module DBLib

open System
open System.Linq
open System.Data
open System.Data.Linq
open Microsoft.FSharp.Data.TypeProviders

[<Literal>]
let connectionString = "Data Source=(local)\sqlserver2014; Initial Catalog=SqlInFsharp; Integrated Security=true;"
type Sql = SqlDataConnection<connectionString>

type DbContext = Sql.ServiceTypes.SimpleDataContextTypes.SqlInFsharp

let removeExistingData (db:DbContext) =
    let truncateTable name =
        sprintf "TRUNCATE TABLE %s" name
        |> db.DataContext.ExecuteCommand
        |> ignore

    ["Customer"; "CustomerImport"; "Country"]
    |> List.iter truncateTable

let insertReferenceData (db:DbContext) =
    ["US","United States";
     "GB","United Kingdon"]
    |> List.iter (fun (code, name) -> 
            let c = new Sql.ServiceTypes.Country()
            c.IsoCode <- code; c.CountryName <- name
            db.Country.InsertOnSubmit c
            )
    db.DataContext.SubmitChanges()

let resetDatabase() =
    use db = Sql.GetDataContext()
    removeExistingData db
    insertReferenceData db

