// -----------------------------------------------------------------------
//   <copyright file="AssemblyInfo.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;


#if NET461
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityTransparent]
[assembly: SecurityRules(SecurityRuleSet.Level1, SkipVerificationInFullTrust = true)]
#endif
