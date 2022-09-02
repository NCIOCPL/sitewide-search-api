#!/bin/bash

# How many seconds do we want this to loop?
runtime=60

# How many parallel tests do we want running?
# Remember this is multiplied by the number of threads used in the command below
paralleltests=2

# What command do you want to run? Remove egrep to see full results
karate="bin/karate -T 3 features/performance | egrep 'failed\:\s*[1-9]'"
#karate="bin/karate -T 3 features/performance"

# What URL do you want to test?
if [ -z "$1" ]
then
  test_host=http://localhost:5000
else
  test_host=$1
fi

export KARATE_APIHOST=$test_host

i="$(date +%s)"
j="$(( i + $runtime ))"
loop=1

while [ $i -lt $j ]
do
  echo "Loop $loop begun"
  for (( k = 0; k < $paralleltests; k++ ))
  do
    (sleep $(echo $[ $RANDOM % 10 + 1 ]/10 | bc -l) && eval $karate && echo "Test $k in loop $loop") &
  done
  wait $(jobs -p)
  loop=$[$loop + 1]
  i="$(date +%s)"
  echo -e "\nTime remaining: $[$j - $i]\n\n"
done
