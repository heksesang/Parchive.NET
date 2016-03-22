namespace Parchive.Library.RecoveryMath

open Parchive.Library.RecoveryMath.Galois

/// PAR2 recovery module.
module Recovery =
    let private table = Table(16, 0x1100B)

    let private constants = [
        let b = table.GetSymbol(2)
        let mutable n = 1
        for i in 0 .. 32767 do
            while n % 3 = 0 || n % 5 = 0 || n % 17 = 0 || n % 257 = 0 do
                n <- n + 1

            yield b ** n
            n <- n + 1
    ]

    /// <summary>
    /// Adds a new set of values to exisiting accumulated recovery data.
    /// </summary>
    /// <param name="values">A collection of <see cref="T:System.Tuple`{T:System.Int32}{T:System.Int32}"/> containing the accumlated recovery data and the new value to add.</param>
    /// <param name="constantIndex">The index of the constant to use. There are 32768 available constants.</param>
    /// <param name="exponent">The exponent of the recovery slice.</param>
    /// <returns>A collection of <see cref="int"/> with the new set of accumulated recovery data.</returns>
    let acc (values:seq<int * int>) (constantIndex:int) (exponent:int) : seq<int> =
        seq {
            for value in values do
                match value with
                | (acc, a) ->
                    let c = constants.[constantIndex]
                    yield (acc + (a * (c ** exponent))).Value
        }