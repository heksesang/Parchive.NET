namespace Parchive.Library.Math

open Parchive.Library.Math.Galois

/// PAR2 recovery module.
module Recovery =
    let private table = Table(16, 0x1100B)

    /// Generates an error code from `slices` and `exp`
    let generateErrorCode (slices:seq<int * int>) (exp:int) : int =
        let values = seq {
            for slice in slices do
                match slice with
                | (x, c) ->
                    yield (table.Mul x (table.Pow c exp))
        }

        let mutable sum = 0;
        for value in values do
            sum <- table.Add sum value;
        sum

    /// Restores data from `slices`
    let restoreData (slices : seq<int * int * int>) =
        let c = seq {
            for row in slices do
                match row with
                | (_, exp, result) ->
                    yield seq {
                        for col in slices do
                            match col with
                            | (constant, _, _) ->
                                yield (table.Pow constant exp)
                    }
        }

        let coefficients = Galois.matrix c table

        let r = seq {
            for row in slices do
                match row with
                | (_, _, result) ->
                    let list = [ result ]
                    yield seq {
                        for x in list do
                            yield x
                    }
        }

        let results = Galois.matrix r table

        Seq.toArray (seq {
            for s in coefficients.GetInverse() * results do
                yield s
        })