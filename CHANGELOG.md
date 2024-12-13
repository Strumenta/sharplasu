# Changelog
All notable changes to this project will be documented in this file.

## 1.0.0
- Added Code Generation support

## 0.9.4
- Downgrade MSTest and remove support for .NETStandard 2.1 to solve potential compatibility issues with some platforms, IDEs
- Remove automatically-added dependency to Microsoft.CSharp
- Fix bugs in StringSource and SimpleOrigin

## 0.9.3
- Fix bugs in Origin and StringSource classes
- Fix documentation format

## 0.9.2
- Make AssertASTsAreEqual more stringent. Previously null properties were ignored in the comparison, now if one is null and the compared one is not, an error is raised

## 0.9.1
- Removed dependency from MSTest.TestFramework in base library. Previously used to implement assert methods for symbol resolution