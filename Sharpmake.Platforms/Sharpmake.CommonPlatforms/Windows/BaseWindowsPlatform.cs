﻿// Copyright (c) 2017 Ubisoft Entertainment
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System.Collections.Generic;
using Sharpmake.Generators;

namespace Sharpmake
{
    public static partial class Windows
    {
        public abstract class BaseWindowsPlatform : BaseMicrosoftPlatform, IWindowsFastBuildCompilerSettings
        {
            #region IWindowsFastBuildCompilerSettings implementation
            public override bool IsPcPlatform => true;
            public IDictionary<DevEnv, string> BinPath { get; set; } = new Dictionary<DevEnv, string>();
            public IDictionary<DevEnv, string> LinkerPath { get; set; } = new Dictionary<DevEnv, string>();
            public IDictionary<DevEnv, string> LinkerExe { get; set; } = new Dictionary<DevEnv, string>();
            public IDictionary<DevEnv, string> LibrarianExe { get; set; } = new Dictionary<DevEnv, string>();
            public IDictionary<DevEnv, string> ResCompiler { get; set; } = new Dictionary<DevEnv, string>();
            public IDictionary<DevEnv, Strings> ExtraFiles { get; set; } = new Dictionary<DevEnv, Strings>();
            #endregion

            #region IPlatformVcxproj implementation
            public override IEnumerable<string> GetImplicitlyDefinedSymbols(IGenerationContext context)
            {
                var defines = new List<string>(base.GetImplicitlyDefinedSymbols(context));

                switch (context.Configuration.Output)
                {
                    case Project.Configuration.OutputType.Exe:
                        context.SelectOption(
                            Options.Option(Options.Vc.Linker.SubSystem.Console, () => { defines.Add("_CONSOLE"); }),
                            Options.Option(Options.Vc.Linker.SubSystem.Application, () => { defines.Add("_WINDOWS"); }));
                        break;
                    case Project.Configuration.OutputType.Lib:
                        defines.Add("_LIB");
                        break;
                    case Project.Configuration.OutputType.Dll:
                        context.SelectOption(
                            Options.Option(Options.Vc.Linker.DLLDefine.Regular, () => { defines.Add("_USRDLL"); }),
                            Options.Option(Options.Vc.Linker.DLLDefine.Extension, () => { defines.Add("_AFXDLL"); defines.Add("_AFXEXT"); }));
                        break;
                    case Project.Configuration.OutputType.None:
                    default:
                        break;
                }

                return defines;
            }
            public override bool HasUserAccountControlSupport => true;

            public override IEnumerable<string> GetPlatformLibraryFiles(IGenerationContext context)
            {
                yield return "kernel32.lib";
                yield return "user32.lib";
                yield return "gdi32.lib";
                yield return "winspool.lib";
                yield return "comdlg32.lib";
                yield return "advapi32.lib";
                yield return "shell32.lib";
                yield return "ole32.lib";
                yield return "oleaut32.lib";
                yield return "uuid.lib";
                yield return "odbc32.lib";
                yield return "odbccp32.lib";
            }

            public override void SetupSdkOptions(IGenerationContext context)
            {
                var conf = context.Configuration;
                var devEnv = context.DevelopmentEnvironment;

                // vs2012 and vs2013 do not support overriding windows kits using the underlying variables
                // so we need to change the VC++ directories path
                if ((devEnv == DevEnv.vs2012 || devEnv == DevEnv.vs2013) && !KitsRootPaths.UsesDefaultKitRoot(devEnv))
                {
                    var options = context.Options;
                    options["ExecutablePath"] = devEnv.GetWindowsExecutablePath(conf.Platform);
                    options["IncludePath"] = devEnv.GetWindowsIncludePath();
                    options["LibraryPath"] = devEnv.GetWindowsLibraryPath(conf.Platform, Util.IsDotNet(conf) ? conf.Target.GetFragment<DotNetFramework>() : default(DotNetFramework?));
                    options["ExcludePath"] = devEnv.GetWindowsIncludePath();
                }
            }
            #endregion
        }
    }
}
