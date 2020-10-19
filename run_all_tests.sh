#!/bin/bash

rm .nugout.txt

GODOT=$(command -v godot)

if [ $? -eq 1 ]
then
  GODOT=./godot
fi

"$GODOT" --no-window -q --build-solutions
"$GODOT" --no-window -q -s addons/NUG/Internal/CliTestRunner.cs

if [ $? -eq 0 ]
then
  cat .nugout.txt
  echo -e '\u001b[32mAll tests passed\u001b[0m'
  exit 0
else
  cat .nugout.txt
  echo -e '\u001b[31mTest(s) failed\u001b[0m'
  exit 1
fi
