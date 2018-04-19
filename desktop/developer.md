Style Guidelines
================

This document is inspired by the [Microsoft's C# Coding Conventions
](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions).


Indentation
-----------

Use 4 spaces indentation.


Switch statements have the case at the same indentation as the switch:

``` csharp
switch (x) {
case 'a':
    ...
case 'b':
    ...
}
```

Nesting
----------

Use Linus Torvalds's trick to reduce code nesting. Many times in a loop, you will find yourself doing a test, and if the test is true, you will nest. Many times this can be changed. Example:

**Do:**

``` csharp
for (i = 0; i < 10; i++) 
{
    if (!foo(i))
        continue;

    bar();
}
```

**Don't:**
``` csharp
for (i = 0; i < 10; i++) 
{
    if (foo(i)) 
    {
        bar();
    }
}
```

Where to put spaces
-------------------

**Do:**

``` csharp
method(a);
array[10];
```

**Don't**:

``` csharp
method ( a );
array [ 10 ];
```

Do not put a space between the generic types, ie:

**Do:**

``` csharp
var list = new List<int>();
```

**Don't**:

``` csharp
var list = new List <int> ();
```

Where to put braces
-------------------

Excepted a few special cases, always put opening braces on their own line (basically, let ctrl+k+d take care of it):

**Do:**

``` csharp
if (a) 
{
    code();
    code();
}
```

**Don't**:

``` csharp
if (a) {
    code();
    code();
}
```

For very small properties, you can compress things:

**Acceptable:**

``` csharp
int Property {
    get { return value; }
    set { x = value; RaisePropertyChanged(); } // Acceptable, since RaisePropertyChanged() is widely used
}
```

 Empty methods: 

**Do:**

``` csharp
void EmptyMethod() {}
```


Multiline Parameters
--------------------

When you need to write down parameters in multiple lines, indent the parameters to be below the previous line parameters, like this:

**Do:**

``` csharp
WriteLine(foo,
          bar,
          baz);
```

**Atrocious**:

``` csharp
WriteLine(foo
          , bar
          , baz);
```

Comments
------------------

Comments start with a space

**Do:**
``` csharp
// Glorious comment
```

**Don't:**
``` csharp
//What an insult
```
For long, multiline comments:


**Do:**
``` csharp
//
// Blah
// Blah again
// and another Blah
//
```

**Don't:**
``` csharp
/*
 * Blah
 * Blah again
 * and another Blah
 */
```

Naming convention
------

* **Argument names** should be `lowerCamelCased`
* **Attributes**, private and public, should be `UpperCamelCased`
* **Instance fields** should be `lowerCamelCased`, and **never start** with an underscore or `m_` prefix
* **Methods** should be `UpperCamelCased`
* **Static fields** should be `UpperCamelCased`
* **Constants** should be `UPPER_SNAKE_CASED` 

Use of `this`
----

The use of "this." as a prefix in code is discouraged, it is mostly redundant.


An exception is made for "this" when the parameter name is the same as an instance variable:

**Do:**

``` csharp
class Message 
{
     char text;
 
     public Message(string text)
     {
         this.text = text;
     }
}
```

Initializing Instances
----------------------

Use the new C# syntax to initialize newly created objects.

**Do:**

``` csharp
     var x = new Foo () {
         Label = "This",
         Color = Color.Red
     };
```

**Don't**:

``` csharp
     var x = new Foo ();
     x.Label = "This";
     x.Color = Color.Red;
```

Accessors
--------------

Always explicitly specify the accessors.

Use of var
----------

Use var on the left-hand side of an assignment when the type name obvious:


**Do:**

``` csharp
    var monkeyUUID = new NSUuid(uuid);
    NSUuid something = DoStuff();
```

**Don't**:

``` csharp
    NSUuid monkeyUUID = new NSUuid(uuid);
    var something = DoStuff();

```