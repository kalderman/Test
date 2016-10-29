namespace Lib.Tests

open System

module ``Foo Tests`` = 
  open Xunit
  open Lib
  
  [<Fact>]
  let ``Foo returns correct result`` () =
    let sut = new Foo()
    let expected = "FooBar"
    let actual = sut.Bar()

    Assert.Equal(expected, actual)
