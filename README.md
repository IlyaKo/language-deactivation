![English](https://img.shields.io/badge/English-gray) [![Russian](https://img.shields.io/badge/Русский-blue)](README.ru.md)

# Language Deactivation

A utility designed to manage the active input language list on Windows.

# Purpose

This tool is intended for users who have specific input languages installed that are rarely used (e.g., for occasional typing, spellchecking, or testing) but do not want them to appear in the standard keyboard layout cycle (`Win` + `Space`) during normal workflow.

It allows "soft deactivation" — the language pack remains on the system, but the input method is removed from the active user list.

# Technical Details

The application acts as a wrapper around the Windows PowerShell command `Set-WinUserLanguageList`

**Operating System:** Windows 10/11 (x64)

**Framework:** .NET Windows Forms
