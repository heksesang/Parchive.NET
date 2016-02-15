// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "GaloisField.fs"
open Parchive.Library.Math

// Define your library scripting code here

let table = GaloisField.GaloisTable(16, 0x1100B)

//let a = GaloisField.GfMatrix.ofSeqSeq [[ 2; 1; ]
//                                       [ 5; 3; ]] table
//
//let b = GaloisField.GfMatrix.ofSeqSeq [[ 4; 1; ]
//                                       [ 4; 3; ]] table
//
//let c = GaloisField.GfMatrix.ofSeqSeq [[ 4; 2; ]
//                                       [ 4; 5; ]] table
//
//printfn "%d %d %d" (a.GetDeterminant()) (b.GetDeterminant()) (c.GetDeterminant())

printfn "%d" (table.Add (table.Mul 0 10) (table.Add (table.Mul 0 6) (table.Mul 2 0)))
printfn "%d" (table.Add (table.Mul 0 4) (table.Add (table.Mul 0 2) (table.Mul 2 0)))
printfn "%d" (table.Add (table.Mul 0 28) (table.Add (table.Mul 0 8) (table.Mul 2 3)))

let a = GaloisField.GfMatrix.ofSeqSeq [[ 4; 2; 1; ]
                                       [ 4; 5; 3; ]
                                       [ 2; 0; 0; ]] table

let d = a.GetDeterminant()

let b = GaloisField.GfMatrix.ofSeqSeq [[ 0; 6; 10; ]
                                       [ 0; 2; 4; ]
                                       [ 3; 8; 28; ]] table

let c = GaloisField.GfMatrix.ofSeqSeq [[  0; 0;  3; ]
                                       [  6; 2;  8; ]
                                       [ 10; 4; 28; ]] table

let m = (a * c)

printfn "d: %d\n" d

printfn "a:"
printfn "%d %d %d" a.[0, 0] a.[0, 1] a.[0, 2]
printfn "%d %d %d" a.[1, 0] a.[1, 1] a.[1, 2]
printfn "%d %d %d\n" a.[2, 0] a.[2, 1] a.[2, 2]

printfn "a^-1:"
printfn "%d %d %d" c.[0, 0] c.[0, 1] c.[0, 2]
printfn "%d %d %d" c.[1, 0] c.[1, 1] c.[1, 2]
printfn "%d %d %d\n" c.[2, 0] c.[2, 1] c.[2, 2]

printfn "a * a^-1:"
printfn "%d %d %d" m.[0, 0] m.[0, 1] m.[0, 2]
printfn "%d %d %d" m.[1, 0] m.[1, 1] m.[1, 2]
printfn "%d %d %d\n" m.[2, 0] m.[2, 1] m.[2, 2]