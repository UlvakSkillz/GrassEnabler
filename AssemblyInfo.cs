using MelonLoader;
using BuildInfo = GrassEnabler.ModBuildInfo;

[assembly: MelonInfo(typeof(GrassEnabler.Main), "GrassEnabler", BuildInfo.Version, "UlvakSkillz")]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]
[assembly: MelonColor(255, 195, 0, 255)]
[assembly: MelonAuthorColor(255, 195, 0, 255)]
[assembly: VerifyLoaderVersion(0, 7, 2, true)]
[assembly: MelonAdditionalDependencies("UIFramework")]

