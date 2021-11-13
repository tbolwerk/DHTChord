module Kata (highAndLow) where
import Data.Char
import Data.List

getIntsOfChars :: String -> [Int]
getIntsOfChars [] = []
getIntsOfChars (' ':input) = getIntsOfChars input
-- getIntsOfChars ('-':x:input) = (digitToInt (concat (getNumber input)) * (- 1)) : getIntsOfChars input
getIntsOfChars (x:input) = digitToInt x : getIntsOfChars input

highAndLow :: String -> String
highAndLow input = show lastElement ++ " " ++ show firstElement
 where array = sort $ getIntsOfChars input
       firstElement = head array
       lastElement  = last array


getNumber [] = []
getNumber (' ':_) = []
getNumber (x:xs) = [x] ++ (getNumber xs)