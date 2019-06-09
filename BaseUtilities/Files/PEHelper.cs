using System;
using System.IO;

public static class PortableExecutableHelper
{
    public static bool IsDotNetAssembly(string peFile)
    {
        try
        {
            System.Reflection.AssemblyName testAssembly =
                System.Reflection.AssemblyName.GetAssemblyName(peFile);

            return true;
        }

        catch (System.IO.FileNotFoundException)
        {
            System.Console.WriteLine("The file cannot be found.");
        }

        catch (System.BadImageFormatException)
        {
            System.Console.WriteLine("The file is not an assembly.");
        }

        return false;
    }
}