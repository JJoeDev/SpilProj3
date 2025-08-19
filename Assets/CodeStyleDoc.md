# Code Style Document



## Kapitler

* [Klasser](#klasser)

* [Variabler](#variabler)

* [Funktioner / Methods](#funktioner-/-methods)

* [IEnumerators](#ienumerators)

## Klasser

Klasse navne bruger camel-case som starter med et stort bogstav og har et stort bogstav hvert mellemrum

```cs
class MyClass
{
    
}
```



## Variabler

Alle variabler skrives med pascal-case som starter med smot og har stort bogstav hvert mellemrum

```cs
class MyClass
{
    // Private variabler bruger m_ for at notere at de er members
    private float m_myFloat;
    [SerializeField] private float m_myFloat2;

    // Public variabler har der imod ingen m_ prefix
    public int myInt;
}
```



## Funktioner / Methods

Funktioner og methods gør også brug af camel-case

```cs
void MyFunction()
{
    
}
```



## IEnumerators

IEnumerators har prefikset ie uden noget _

```cs
IEnumerator ieMyFunc()
{
    
}
```




