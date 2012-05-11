 ::BEGIN FUNCTION::::::::::::::::::::::::::::::::::::::::
 @ECHO OFF
 
 :DeQuote
 SET DeQuote.Variable=%1
 CALL Set DeQuote.Contents=%%%DeQuote.Variable%%%
 Echo.%DeQuote.Contents%|FindStr/brv ""^">NUL:&&Goto :EOF
 Echo.%DeQuote.Contents%|FindStr/erv ""^">NUL:&&Goto :EOF
 
 Set DeQuote.Contents=####%DeQuote.Contents%####
 Set DeQuote.Contents=%DeQuote.Contents:####"=%
 Set DeQuote.Contents=%DeQuote.Contents:"####=%
 Set %DeQuote.Variable%=%DeQuote.Contents%
 
 Set DeQuote.Variable=
 Set DeQuote.Contents=
 Goto :EOF
 :: Written by Frank P. Westlake, 2001.09.22, 2001.09.24
 :: Modified by Simon Sheppard 2002.06.09
 :::::::::::::::::::::::::::::::::::::::::::::::::::::::