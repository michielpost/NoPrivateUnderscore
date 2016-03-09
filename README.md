[![Build status](https://ci.appveyor.com/api/projects/status/4yllghl99yr1e4yi?svg=true)](https://ci.appveyor.com/project/michielpost/noprivateunderscore)
# NoPrivateUnderscore
Roslyn analyzer that returns an error for private fields that start with an underscore

Personally I like starting private fields with an underscore, but for one of the projects I'm working on it's against the coding guidelines.

Example:  
`private string _myString`

Will be renamed to:  
`private string myString` 
