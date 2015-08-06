System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

#r @"packages\FSharp.Data.TypeProviders.0.0.1\lib\net40\FSharp.Data.TypeProviders.dll"
#r @"System.Data.Linq.dll"

open System
open System.Linq
open System.Data
open Microsoft.FSharp.Data.TypeProviders

[<Literal>]
let sourceConnectionString = "Data Source=(local)\sqlserver2014; Initial Catalog=SqlInFsharp; Integrated Security=true;"

[<Literal>]
let targetConnectionString = "Data Source=(local)\sqlserver2014; Initial Catalog=SqlInFsharp; Integrated Security=true;"


type SourceSql = SqlDataConnection<sourceConnectionString>
type TargetSql = SqlDataConnection<targetConnectionString>

let makeName first last =
    sprintf "%s %s" first last

let makeBirthDate (age:Nullable<int>) =
    if age.HasValue then
        Nullable (DateTime.Today.AddYears(-age.Value))
    else
        Nullable()

let makeTargetCustomer (sourceCustomer:SourceSql.ServiceTypes.CustomerImport) =
    let targetCustomer = new TargetSql.ServiceTypes.Customer()
    targetCustomer.Name <- makeName sourceCustomer.FirstName sourceCustomer.LastName
    targetCustomer.Email <- sourceCustomer.EmailAddress
    targetCustomer.Birthdate <- makeBirthDate sourceCustomer.Age
    targetCustomer

let transferAll() = 
    use sourceDb = SourceSql.GetDataContext()
    use targetDb = TargetSql.GetDataContext()

    let insertOne counter customer = 
        targetDb.Customer.InsertOnSubmit customer

        if counter % 1000 = 0 then
            targetDb.DataContext.SubmitChanges()
            printfn "...%i records transfered" counter

    sourceDb.CustomerImport
    |> Seq.map makeTargetCustomer
    |> Seq.iteri insertOne

    targetDb.DataContext.SubmitChanges()
    printfn "Done"

#time
transferAll()
#time

