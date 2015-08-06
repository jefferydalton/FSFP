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

query {
    for c in db.Customer do
    where (c.Email.EndsWith("gmail.com"))
    select c
    count
}
