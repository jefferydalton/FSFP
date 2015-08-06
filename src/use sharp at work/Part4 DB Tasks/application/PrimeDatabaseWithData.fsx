
System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

#r @"Packages\FsCheck.2.0.5\lib\net45\FsCheck.dll"


#r @"packages\FSharp.Data.TypeProviders.0.0.1\lib\net40\FSharp.Data.TypeProviders.dll"
#r @"System.Data.Linq.dll"

open System
open System.Linq
open System.Data
open Microsoft.FSharp.Data.TypeProviders
open FsCheck

[<Literal>]
let connectionString = "Data Source=(local)\sqlserver2014; Initial Catalog=SqlInFsharp; Integrated Security=true;"

type Sql = SqlDataConnection<connectionString>

let possibleFirstNames =
    ["Merissa";"Kenneth";"Zora";"Oren"]
let possibleLastNames =
    ["Applewhite";"Feliz";"Abdulla";"Strunk"]

let generateFirstName() =
    FsCheck.Gen.elements possibleFirstNames

let generateLastName() =
    FsCheck.Gen.elements possibleLastNames

let generateEmail() =
    let userGen = FsCheck.Gen.elements ["a";"b";"c";"d";"e";"f"]
    let domainGen = FsCheck.Gen.elements ["gmail.com";"example.com";"outlook.com"]
    let makeEmail u d = sprintf "%s@%s" u d
    FsCheck.Gen.map2 makeEmail userGen domainGen

let generateAge() =
    let nonNullAgeGenerator = 
        FsCheck.Gen.choose(1, 99)
        |> FsCheck.Gen.map (fun age -> Nullable age)
    let nullAgeGenerator =
        FsCheck.Gen.constant (Nullable())

    FsCheck.Gen.frequency [
        (19,nonNullAgeGenerator)
        (1, nullAgeGenerator)]

let createCustomerImport first last email age =
    let c = new Sql.ServiceTypes.CustomerImport()
    c.FirstName <- first
    c.LastName <- last
    c.EmailAddress <- email
    c.Age <- age
    c

let generateCustomerImport =
    createCustomerImport 
    <!> generateFirstName()
    <*> generateLastName()
    <*> generateEmail()
    <*> generateAge()

let insertAll = 
    use db = Sql.GetDataContext()

    db.DataContext.Log <- Console.Out

    let insertOne counter customer =
        db.CustomerImport.InsertOnSubmit customer

        if counter % 1000 = 0 then
            db.DataContext.SubmitChanges()

        let count = 1000
        let generator = FsCheck.Gen.sample 0 count generateCustomerImport

        generator |> List.iteri insertOne
        db.DataContext.SubmitChanges()

#time
insertAll()
#time
