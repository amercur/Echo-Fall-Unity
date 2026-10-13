using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
public static class Phase7PackagingAudit
{
    public static object Run()
    {
        var references=new List<string>();var errors=new List<string>();
        foreach(string path in Directory.GetFiles("Builds/Windows/Echo-Fall_Data/Managed","*.dll"))
            try{foreach(var reference in Assembly.ReflectionOnlyLoadFrom(Path.GetFullPath(path)).GetReferencedAssemblies())if(reference.Name=="System.Windows.Forms")references.Add(Path.GetFileName(path));}catch(Exception e){errors.Add(Path.GetFileName(path)+": "+e.Message);}
        return new{windowsFormsReferences=references,errors};
    }
}
