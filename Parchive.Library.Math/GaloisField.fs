namespace Parchive.Library.Math.Galois

/// Galois field table
type Table(power:int, generator:int) =
    let size = (int)((float)2 ** (float)power);
    let limit = size - 1;
    let log : int[] = Array.zeroCreate (size)
    let antilog : int[] = Array.zeroCreate (size)
        
    let shift value =
        let shifted = (value <<< 1)
            
        if (shifted &&& size) <> 0 then
            shifted ^^^ generator
        else
            shifted

    let rec build_table mask depth : unit =
        log.[mask] <- depth
        antilog.[depth] <- mask

        if depth <> limit then
            build_table (shift mask) (depth + 1)

    do
        log.[0] <- limit
        antilog.[limit] <- 0

        build_table 1 0

    override this.Equals (arg) =
        match arg with
        | :? Table as other -> 
            this.Size = other.Size && this.Generator = other.Generator
        | _ -> false

    override this.GetHashCode() =
        this.Generator * this.Size
        
    /// Multiplies `a` with `b`, where `a` and `b` exist in the field of this table.
    member this.Mul a b =
        if a < 0 || a > limit || b < 0 || b > limit then invalidArg "" "The arguments need to exist in the field."
        if a = 0 || b = 0 then
            0
        else
            let sum = log.[a] + log.[b]
            if sum >= limit then
                antilog.[sum - limit]
            else
                antilog.[sum]
                
    /// Divides `a` by `b`, where `a` and `b` exist in the field of this table.
    member this.Div a b =
        if a < 0 || a > limit || b < 0 || b > limit then invalidArg "" "The arguments need to exist in the field."
        if b = 0 then
            raise (new System.DivideByZeroException())

        if a = 0 then
            0
        else
            let sum = log.[a] - log.[b]
            if sum < 0 then
                antilog.[sum + limit]
            else
                antilog.[sum]

    /// Raises `a` to the power of `b`, where `a` and `b` exist in the field of this table.
    member this.Pow a b =
        if a < 0 || a > limit || b < 0 || b > limit then invalidArg "" "The arguments need to exist in the field."
        if b = 0 then 0
        else if a = 0 then 0
        else
            let sum = log.[a] * b
            let sum = (sum >>> power) + (sum &&& limit)
            if (sum >= limit) then
                antilog.[sum-limit]
            else
                antilog.[sum]
                
    /// Add `a` to `b`, where `a` and `b` exist in the field of this table.
    member this.Add a b =
        if a < 0 || a > limit || b < 0 || b > limit then invalidArg "" "The arguments need to exist in the field."
        a ^^^ b

    /// Substracts `b` from `a`, where `a` and `b` exist in the field of this table
    member this.Sub a b =
        if a < 0 || a > limit || b < 0 || b > limit then invalidArg "" "The arguments need to exist in the field."
        a ^^^ b

    /// The field size of the table
    member this.Size = size;

    /// The generator of the table
    member this.Generator = generator;



/// Galois field matrix type
[<NoEquality; NoComparison>]
type Matrix(values:int[,], table:Table) =
    /// The galois table.
    member m.Table = table;

    /// Converts Matrix `m` to int[,]
    static member op_Explicit(m:Matrix) =
        m.ToArray2D
    
    /// Gets the number of rows of this matrix
    member m.Rows =
        values.GetLength 0
    
    /// Gets the number of columns of this matrix
    member m.Cols =
        values.GetLength 1
    
    /// The entry of this matrix at row `i` and column `j`
    member m.Item
        with get (i, j) =
            values.[i, j]
        and set (i, j) v =
            values.[i, j] <- v
    
    /// Converts this matrix into a 2d array
    member m.ToArray2D() =
        values
    
    /// Converts this matrix into a jagged array, i.e. from Matrix to int[][]
    member m.ToArray() =
        let a = m.ToArray2D()
        [|for i = 0 to m.Rows - 1 do yield [|for j = 0 to m.Cols - 1 do yield a.[i, j]|]|]
    
    /// Converts this matrix into a one dimensional sequence, scanning columns from left to right and rows from top to bottom
    member m.ToSeq() =
        let a = m.ToArray()
        seq {for aa in a do yield! Seq.ofArray aa}
    
    /// Returns an enumerator that iterates through this matrix
    member m.GetEnumerator() =
        m.ToSeq().GetEnumerator()
    
    /// Creates a copy of this matrix
    member m.Copy() = 
        Matrix((Array2D.copy values), table)

    /// Gets a submatrix of this matrix, excluding row `x` and column `y`
    member m.GetSubMatrix(x, y) =
        Matrix((Array2D.init (m.Rows-1) (m.Cols-1) (fun i j ->
            if j >= y && i >= x then
                m.[i+1, j+1]
            else
                if i >= x then
                    m.[i+1, j]
                else
                    if j >= y then
                        m.[i, j+1]
                    else
                        m.[i, j]
            )), m.Table)
    
    /// Gets the determinant of this matrix
    member m.GetDeterminant() =
        if (m.Rows <> m.Cols) then invalidArg "" "Cannot compute the determinant of a nonsquare matrix."

        let d = seq {
            match m.Cols with
            | 0 -> yield 0
            | 1 -> yield m.[0, 0]
            | 2 -> yield m.Table.Mul m.[0, 0] m.[1, 1]
                   yield m.Table.Mul m.[0, 1] m.[1, 0]
            | _ -> for c in 0 .. (m.Cols - 1) do
                    let s = m.GetSubMatrix(0, c)
                    yield (m.Table.Mul m.[0, c] (s.GetDeterminant()))
        }

        Seq.reduce (fun acc x -> m.Table.Add acc x) d

    /// Gets the inverse of this matrix
    member m.GetInverse() =
        if (m.Rows <> m.Cols) then invalidArg "" "Cannot compute the inverse of a nonsquare matrix."
        let mm = array2D (seq {
            for i in 0 .. (m.Rows-1) do
                yield seq {
                    for j in 0 .. (m.Cols-1) do
                        yield (m.GetSubMatrix(i, j).GetDeterminant())
                }
                    
        })
        Matrix(mm, m.Table).GetTranspose() / m.GetDeterminant()

    /// Gets the transpose of this matrix
    member m.GetTranspose() =
        Matrix(Array2D.init m.Cols m.Rows (fun i j -> m.[j, i]), m.Table)
    
    /// Adds matrix `a` to matrix `b`
    static member (+) (a:Matrix, b:Matrix):Matrix =
        if (a.Rows <> b.Rows) || (a.Cols <> b.Cols) || a.Table.Size <> b.Table.Size || a.Table = b.Table then invalidArg "" "Cannot add matrices of different sizes."
        Matrix((Array2D.init a.Rows a.Cols (fun i j -> a.Table.Add a.[i, j] b.[i, j])), a.Table)
    
    /// Subtracts matrix `b` from matrix `a`
    static member (-) (a:Matrix, b:Matrix):Matrix =
        if (a.Rows <> b.Rows) || (a.Cols <> b.Cols) || a.Table <> b.Table then invalidArg "" "Cannot subtract matrices of different sizes."
        Matrix((Array2D.init a.Rows a.Cols (fun i j -> a.Table.Sub a.[i, j] b.[i, j])), a.Table)
    
    /// Multiplies matrix `a` and matrix `b` (matrix product)
    static member (*) (a:Matrix, b:Matrix):Matrix =
        if (a.Cols <> b.Rows) || a.Table <> b.Table then invalidArg "" "Cannot multiply two matrices of incompatible sizes."
        Matrix((Array2D.init a.Rows b.Cols (fun i j ->
            let r = [| 0 .. (b.Rows - 1) |]
            r.[0] <- a.Table.Mul a.[i, 0] b.[0, j]
            Seq.reduce (fun acc x -> a.Table.Add acc (a.Table.Mul a.[i, x] b.[x, j])) r )), a.Table)
    
    /// Hadamard product of matrix `a` and matrix `b`
    static member (.*) (a:Matrix, b:Matrix):Matrix =
        if (a.Rows <> b.Rows) || (a.Cols <> b.Cols) || a.Table <> b.Table then invalidArg "" "Cannot multiply matrices of different sizes."
        Matrix(Array2D.init a.Rows a.Cols (fun i j -> a.Table.Mul a.[i, j] b.[i, j]), a.Table)
    
    /// Hadamard division of matrix `a` by matrix `b`
    static member (./) (a:Matrix, b:Matrix):Matrix =
        if (a.Rows <> b.Rows) || (a.Cols <> b.Cols) || a.Table <> b.Table then invalidArg "" "Cannot divide matrices of different sizes."
        Matrix((Array2D.init a.Rows a.Cols (fun i j -> a.Table.Div a.[i, j] b.[i, j])), a.Table)
    
    /// Adds scalar `b` to each element of matrix `a`
    static member (+) (a:Matrix, b:int):Matrix =
        Matrix (Array2D.map (fun x -> a.Table.Add x b) (a.ToArray2D()), a.Table)
    /// Adds scalar `a` to each element of matrix `b`
    static member (+) (a:int, b:Matrix):Matrix =
        Matrix (Array2D.map (fun x -> b.Table.Add x a) (b.ToArray2D()), b.Table)
    
    /// Subtracts scalar `b` from each element of matrix `a`
    static member (-) (a:Matrix, b:int):Matrix =
        Matrix (Array2D.map (fun x -> a.Table.Sub x b) (a.ToArray2D()), a.Table)
    /// Subtracts each element of of matrix `b` from scalar `a`
    static member (-) (a:int, b:Matrix):Matrix =
        Matrix (Array2D.map (fun x -> b.Table.Sub x a) (b.ToArray2D()), b.Table)
    
    /// Multiplies each element of matrix `a` by scalar `b`
    static member (*) (a:Matrix, b:int):Matrix =
        Matrix (Array2D.map (fun x -> a.Table.Mul x b) (a.ToArray2D()), a.Table)
    /// Multiplies each element of matrix `b` by scalar `a`
    static member (*) (a:int, b:Matrix):Matrix =
        Matrix (Array2D.map (fun x -> b.Table.Mul x a) (b.ToArray2D()), b.Table)
    
    /// Divides each element of matrix `a` by scalar `b`
    static member (/) (a:Matrix, b:int):Matrix =
        Matrix (Array2D.map (fun x -> a.Table.Div x b) (a.ToArray2D()), a.Table)
    /// Creates a matrix whose elements are scalar `a` divided by each element of matrix `b`
    static member (/) (a:int, b:Matrix):Matrix =
        Matrix (Array2D.map (fun x -> b.Table.Div x a) (b.ToArray2D()), b.Table)

[<RequireQualifiedAccess>]
module Galois =
    /// Creates a galois matrix from sequence of sequences `s` and galois table `t`
    let matrix (s:seq<seq<int>>) (t:Table) :Matrix = (s |> array2D, t) |> Matrix

    /// Creates a galois table with power `p` and generator `g`
    let table (p:int) (g:int) = Table(p, g)