# Changelog
All notable changes to this project will be documented in this file.

## 0.9.3
- Fix bugs in Origin and StringSource classes
- Fix documentation format

## 0.9.2
- Make AssertASTsAreEqual more stringent. Previously null properties were ignored in the comparison, now if one is null and the compared one is not, an error is raised

## 0.9.1
- Removed dependency from MSTest.TestFramework in base library. Previously used to implement assert methods for symbol resolution