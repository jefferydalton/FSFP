module TestCustomer

open System
open System.Linq
open System.Data
open System.Data.Linq
open Microsoft.FSharp.Data.TypeProviders
open NUnit.Framework

[<Test>]
let ``When upsert customer called with null id , expect customer created with new id``() =
    DBLib.resetDatabase()
    use db = DBLib.Sql.GetDataContext()
    
    let newId = db.Up_Customer_Upsert(Nullable(),"Alice","x@example.com",Nullable())
    
    Assert.Greater(newId,0)

    let customerCount = db.Customer |> Seq.length
    Assert.AreEqual(1, customerCount)

[<Test>]
let ``When upsert customer called with existing id, expect customer updated``() =
    DBLib.resetDatabase()
    use db = DBLib.Sql.GetDataContext()

    let custId = db.Up_Customer_Upsert(Nullable(),"Alice","x@example.com",Nullable())

    let newId = db.Up_Customer_Upsert(Nullable custId,"Bob","y@example.com",Nullable())

    Assert.AreEqual(custId,newId)

    let customerCount = db.Customer |> Seq.length
    Assert.AreEqual(1, customerCount)

    let customer = db.Customer |> Seq.head
    Assert.AreEqual("Bob",customer.Name)

[<Test>]
let ``When upsert customer called with blank name, expect validation error``() =
    DBLib.resetDatabase()

    use db = DBLib.Sql.GetDataContext()

    try
        db.Up_Customer_Upsert(Nullable(),"","x@example.com",Nullable()) |> ignore
        Assert.Fail("expecting a SQLException")
    with
    | :? System.Data.SqlClient.SqlException as ex ->
        Assert.That(ex.Message,Is.StringContaining("@Name"))
        Assert.That(ex.Message,Is.StringContaining("blank"))