
# ColafeScript

**ColafeScript** (**CO**nsole **LA**nguage **F**or **E**vents) **is a simple runtime scripting language for the Unity game engine.** It was originally designed to be used as an alternative to the (*limited*) UnityEvent inspector fields and evolved into **a language for scripting chains of event functions**. It also makes up for a good **in-game console language** for testing functions, setting up variables, starting event chains, or just enabling cheats.

## What does it do?

 - **Call component functions directly**, *you can even pass arguments!*
 - **Select objects by name, path and/or type**; call their methods with ease.
 - **Invoke async functions** from any type of component.
 - Create and access **Local and Global Variables** from anywhere.
 - Write **Conditional Statements** with shorthand logic gates (&&, ||) + the usual (==, !=, <, <=, >, >=) comparison operators + the logic not (!).
 - Supports **Loop Statements** of the while kind.
 - Supports **all the common arithmetic operators** (+, -, \*, /, %), including powers (\*\*) and bitwise (~,&,|,^,<<,>>).
 - Also supports **Strings** and concatenation.

[But why did you make this???!!!](#why?)

## Examples

I'm still writting the documentation.
In the meantime here are a couple of commented code examples to guide you through the language:

```c
// This is a line comment

/* You can type
multiline coments
like this */

// ASSIGNMENT OPERATORS AND VARIABLES

// Variable names can have letters, digits and underscores, the first character whoever can't be a digit.

foobar = 5;         // Assign a value to a LOCAL variable, in this case foobar is being defined.
foobar $= 6;        // Assigns a value only if the variable is undefined, because foobar has been defined it won't get assigned again.
.globar = 6;        // Assign a value to a GLOBAL variable (the . at the start is ussed to access variables in global memory)
foo = bar = 3.1415; // Multiple variables can be assigned in a row
bitbar = true;      // Assign boolean values
foo = "Now foo contains a string."; // Assign string variables, the characters " \ and ` need to be escaped by putting a \ before them

// SEFL ASSIGNMENT OPERATORS

// Arithmetic
foobar %= 2;
foobar += 5;
foobar -= 5;
foobar *= 5;
foobar /= 2;
foobar **= 2;  // This will raise foobar to the power of 2
++foobar;      // Only prefix incremend and decrement are supported, TL;DR Don't do foo++ or foo--, do ++foo and --foo
bitbar <<= 1;
bitbar >>= 1;
bitbar &= 32767;
bitbar |= 32768;
bitbar ^= 43690;

// EXPRESSIONS

// Variables can be used inside expressions 
foobar = 2**4 * 25/(2+3) * 40/3 * 64 + .globar - 1 -2**16;

// Strings and variables can't be joined using + , instead use ` ` to insert variables into strings
stringvar = "You can insert variables in strings using grave accents `.globar`";

// Bitwise operations use a 16bit unsigned integet (ushort)
bitwisevar = (2 | 4 | 8) >> (1 & 1);

// The result of a boolean expression can be stored inside a variable
booleanvar = .globar < 10;
otherboolvar = $booleanvar; // The dollar sign is an unary boolean operator that returns true if the variable is defined

// CONDITIONAL STATEMENT

? (booleanvar == true && .globar > 5){
    // This is the if statement, curly braces are required if you have more than one statement inside
    --.globar;
    booleanvar = false;
}
:? (.globar < 5) {
    // This is the else if statement, optional and you can add as many as you like after the if
    // There is a single statement in this else if, but you can put curly braces anyways if you want
    ++.globar;
}
: {
    // This is the else statement, also optional
    ++.globar;
}

// LOOP STATEMENT

i = 1;
% (i < 10){
    // This is the while loop, same rules for the curly braces as the if statement
    j $= 1; // Remember, the $= only assigns unassigned variables, so this will only get assigned once.
    j += i;
    ++i;
}

// SIMPLE FUNCTION CALLS

LocalFunctionName[];    // A LOCAL function call, the arguments (if any) go in between the [] separated by commas
                        // (YOU CAN'T PASS A FUNCTION CALL AS AN ARGUMENT OF A FUNCTION CALL)
.GlobalFunctionName[];  // A GLOBAL function call (notice the . at the start)

// BUILT IN FUNCTIONS

// ColafeScript comes with a few built-in functions
// there are more than these, but some are only meant to be called by other scripts.

Echo[foobar];               // Log/prints an expression to console, also returns it
CompareStr[stringvar, foo]; // Compare two strings, returns true if they are the same, false otherwise
Free["j"];                  // Frees a variable from memory, remember to write the quotes
Wipe[];                     // Clears all the variables in local memory
WipeG[];                    // Clears all the variables in global memory

// COMPLEX FUNCTION CALLS

// Complex calls are used to invoke the methods of components that implement IColafeCallable
// Complex calls have a syntax to find one or multiple instances of components and objects

#<ComponentName>FunctionName[];                             // Call the method of the first found instance of a component
#<*ComponentWithFunction>FunctionName[];                    // Call the method in all instances of a component
#"ObjectNameOrPath"<ComponentWithFunction>FunctionName[];   // Call the method of a component inside a specific gameobject, paths are separated by /
#"*ObjectNameOrPath"<ComponentWithFunction>FunctionName[];  // Call the method of a component inside ALL gameobject with the same name or path
#"*ObjectNameOrPath"<*ComponentWithFunction>FunctionName[]; // Call the method of ALL component instances inside ALL gameobject with the same name or path
#"ObjectNameOrPath*"<ComponentWithFunction>FunctionName[];  // Call the method of a component inside a gameobject + all its siblings

// The return value can be stored into a variable as long as the call finds a single instance (TL;DR don't put * if you want to store the return value)

// INVOKE CALLS

// Invoke calls will execute AFTER the script has ended (aka in the next frame or after a given time)
// Invoke calls have a similar syntax as function calls, but they use @ at the start and () instead of []
// Invokable methods don't have arguments, the contents inside () are the start and repeat time (optional)
// Invoke calls can only be done to methods that return void.
// The path is optional but the type in between <> is necesary.

@<*ComponentType>FunctionName(); // Invokes the function FunctionName inside all instances of ComponentType, will execute in the next frame

// Eg. @"Objectname"<ComponentType>FunctionName();     will execute in the next frame, this is the same as putting a 0 in between the ()
// Eg. @"Objectname"<ComponentType>FunctionName(1);    will execute after 1 second
// Eg. @"Objectname"<ComponentType>FunctionName(0, 2); will execute after in the next frame and repeat every 2 seconds

// PATHS SYNTAX

// Paths are used in Complex Function Calls and Invoke Calls to find specific gameobjects
// "Parent/Child" finds an object named Child inside an object named Parent
// "*Parent/Child" finds all objects named Child inside an object named Parent
// "Parent/Child*" finds an object named Child + its siblings, inside an object named Parent
// "Grandparent/Parent/..." finds the first child of an object named Parent inside an object named Grandparent
// "Grandparent/Parent/...*" finds ALL children of an object named Parent inside an object named Grandparent
// "Grandparent/.../Parent" finds an object named Parent somewhere inside the hierarchy of an object named Grandparent
// "*Grandparent/.../Parent" finds ALL objects named Parent somewhere inside the hierarchy of an object named Grandparent
```
 
## Why?

There are a few reasons why ColafeScript exists:

### I just want to call the thing !!!!

*This has probably happened to you a lot:* **You finish a new method** for a component and you want to check if it works... **so you just call it from inside the closest ``Start()`` method you can find; compile, run the game, check if it worked, and if it works... remove it.** Some go as far as to assign a keypress to call the function in order to test it, or call it by clicking a boolean checkbox in the inspector...

**Needless to say this way of testing sucks**, and most programmers would just assume that the right way of doing is to create some unit tests that check if the function works properly, however... you are making a game, and making unit tests for physics, collision checks or state machines is complicated.
So, now **that's where the magic of ColafeScript comes in, just call your functions directly, set the variables you need and see if it works.** You can even use this to make a small suite of unit tests to run whenever you feel like. 

### Do I really need another class just for that?!

The classic: **You want to do something very specific**, a very small thing that fits nowhere. Like check how many enemies are inside a room. But you already have a Room class, a very generic class to handle rooms... **Adding the function there will make it stop being generic...** So you need a EnemyRoom that inherits Room. It's gonna be an exact copy of Room except for that one little method. **Is it really worth the trouble?**

The solution? Forget classes that work like a one-stop store where you can get *just what they offer* and instead make a toolbox you can use to build *what you actually need*. **With ColafeScript you can build small sets of callbacks with logic inside them and call them whenever you need to without having to bother about writing new classes or methods.**

### TurnOnLightsOpenDoorStartConversation23();

Oh so you want to call ``void TurnOnLights(float intensity);`` ,``void OpenDoor();`` and then the ``string StartConversation(string character, string line);`` method?
**But each method is from a different component right?** Well time to write a bunch of ``GetComponent<>()`` , assign them to variables, and either **assign those callbacks to some event or make a function... both with a comically long name like ``TurnOnLightsOpenDoorStartConversation``or similar.**
Want to do that again but change it so that ``void TurnOnLights(float intensity);`` gets called with a different intensity? Well, time to add a **another** function...

**This is the function and event callback nightmare that a lot of heavily scripted games have to deal with**. And if you are doing this inside ``MonoBehaviour`` classes it's very likely you will end up having to make a new .CS file every time you need to make a small change in the way an event gets executed... At this moment **you might be thinking of a few solutions to overcome this problem... *Like UnityEvents!***

Oh wait, *some of those methods take more than one argument*, and **they won't show up in the UnityEvent inspector**. And one of them returns a string... *that won't work either*. Also *how can I add conditions to this without having to make a new method?*
Well that is one of the reasons ColafeScript was created, just replace your UnityEvents in the inspector with plain string variables and write the callbacks there, **ColafeScripts has the sintax to do all the finding of components and calling their methods with the arguments, you can even add conditions, loops, create and assign variables as part of the event. *No compiling, creating new files or functions is required!***

