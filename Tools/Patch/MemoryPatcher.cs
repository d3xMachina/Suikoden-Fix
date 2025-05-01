using HarmonyLib;
using Il2CppInterop.Runtime;
using System;
using System.Runtime.InteropServices;

namespace Suikoden_Fix.Tools.Patch;

partial class MemoryPatcher
{
    private const uint PAGE_EXECUTE_READWRITE = 0x40;

    // MethodInfo struct (partial)
    [StructLayout(LayoutKind.Sequential)]
    public struct Il2CppMethodInfo
    {
        public IntPtr methodPointer;
        // Other fields omitted
    }

    [DllImport("kernel32.dll")]
    private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

    public static void Patch(IntPtr address, int offset, byte[] code)
    {
        if (address == IntPtr.Zero)
        {
            return;
        }

        try
        {
            var targetAddress = address + offset;

            // Change protection to allow writing
            VirtualProtect(targetAddress, (UIntPtr)code.Length, PAGE_EXECUTE_READWRITE, out uint oldProtect);
            
            // Write the bytes
            Marshal.Copy(code, 0, targetAddress, code.Length);
            
            // Restore original protection
            VirtualProtect(targetAddress, (UIntPtr)code.Length, oldProtect, out _);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to patch method at address {address}: {ex}.");
        }
    }

    public static void PatchNOP(IntPtr address, int offset, int length)
    {
        var code = new byte[length];
        Array.Fill(code, (byte)0x90);

        Patch(address, offset, code);
    }

    public static IntPtr GetImage(string image)
    {
        try
        {
            unsafe
            {
                uint size = 0;
                var assemblies = IL2CPP.il2cpp_domain_get_assemblies(IL2CPP.il2cpp_domain_get(), ref size);

                for (int i = 0; i < size; ++i)
                {
                    var currentImage = IL2CPP.il2cpp_assembly_get_image(assemblies[i]);
                    var currentImageNamePtr = IL2CPP.il2cpp_image_get_name(currentImage);
                    var currentImageName = Marshal.PtrToStringAnsi(currentImageNamePtr);

                    if (currentImageName == image)
                    {
                        return currentImage;
                    }
                }
            }
        
            Plugin.Log.LogError($"Failed to get image {image}.");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to get image {image}: {ex}.");
        }

        return IntPtr.Zero;
    }

    public static IntPtr GetClass(IntPtr image, string namespaze, string clazz)
    {
        try
        {
            var classPtr = IL2CPP.il2cpp_class_from_name(image, namespaze, clazz);
            if (classPtr == IntPtr.Zero)
            {
                Plugin.Log.LogError($"Failed to get class {clazz}.");
            }
        
            return classPtr;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to get class {clazz}: {ex}.");
            return IntPtr.Zero;
        }
    }

    public static IntPtr GetMethod(IntPtr clazz, string methodName, Type[] parameters = null, ArgumentType[] variations = null)
    {
        try
        {
            var iter = IntPtr.Zero;
            IntPtr currentMethodPtr;
        
            while ((currentMethodPtr = IL2CPP.il2cpp_class_get_methods(clazz, ref iter)) != IntPtr.Zero)
            {
                var namePtr = IL2CPP.il2cpp_method_get_name(currentMethodPtr);
                var currentMethodName = Marshal.PtrToStringAnsi(namePtr);
            
                if (methodName != currentMethodName)
                {
                    continue;
                }

                int score = 0;

                if (parameters != null)
                {
                    for (int i = 0; ; ++i)
                    {
                        var paramPtr = IL2CPP.il2cpp_method_get_param(currentMethodPtr, (uint)i);
                        if (paramPtr == IntPtr.Zero)
                        {
                            break;
                        }

                        if (i >= parameters.Length)
                        {
                            Plugin.Log.LogWarning($"Method {currentMethodName} has more arguments than expected! Skipping...");
                            score = 0;
                            break;
                        }

                        var typeNamePtr = IL2CPP.il2cpp_type_get_name(paramPtr);
                        var typeName = Marshal.PtrToStringAnsi(typeNamePtr);

                        var variation = "";
                        
                        if (variations != null)
                        {
                            variation = variations[i] switch
                            {
                                ArgumentType.Ref => "&",
                                ArgumentType.Out => "", // TODO
                                ArgumentType.Pointer => "", // TODO
                                _ => ""
                            };
                        }
                
                        if (typeName != parameters[i].FullName + variation)
                        {
                            Plugin.Log.LogWarning($"Argument {i} mismatch in {currentMethodName}!\n" +
                                                  $"Expected: {parameters[i]}\n" +
                                                  $"Actual: {typeName}\n" +
                                                  $"Skipping...");
                            score = 0;
                            break;
                        }
                    
                        ++score;
                    }
                }

                if (parameters == null ||
                    score == parameters.Length)
                {
                    var imageName = "";
                    var namespaceName = "";
                    var className = "";

                    // Get class info
                    var imagePtr = IL2CPP.il2cpp_class_get_image(clazz);
                    if (imagePtr != IntPtr.Zero)
                    {
                        var imageNamePtr = IL2CPP.il2cpp_image_get_name(imagePtr);
                        if (imageNamePtr != IntPtr.Zero)
                        {
                            imageName = Marshal.PtrToStringAnsi(imageNamePtr);
                        }
                    }

                    var namespacePtr = IL2CPP.il2cpp_class_get_namespace(clazz);
                    if (namespacePtr != IntPtr.Zero)
                    {
                        namespaceName = Marshal.PtrToStringAnsi(namespacePtr);
                    }

                    var classNamePtr = IL2CPP.il2cpp_class_get_name(clazz);
                    if (classNamePtr != IntPtr.Zero)
                    {
                        className = Marshal.PtrToStringAnsi(classNamePtr);
                    }

                    // Get method pointer
                    var methodInfo = Marshal.PtrToStructure<Il2CppMethodInfo>(currentMethodPtr);
                
                    Plugin.Log.LogInfo($"Found method: [{imageName}] {namespaceName}.{className}::{methodName} " +
                                       $"at 0x{methodInfo.methodPointer.ToInt64():X}.");
                
                    return methodInfo.methodPointer;
                }
            }
        
            return IntPtr.Zero;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to get method {methodName}: {ex}.");
            return IntPtr.Zero;
        }
    }

    public static IntPtr GetMethodAddress(Type type, string methodName, Type[] parameters = null, ArgumentType[] variations = null)
    {
        try
        {
            if (type.Module == null)
            {
                throw new Exception("Module is null.");
            }

            // Get the IL2CPP image (assembly) containing the type
            var image = GetImage(type.Module.Name);
            if (image == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            var namespaze = type.Namespace != null ? type.Namespace : "";
            var classPtr = GetClass(image, namespaze, type.Name);
            if (classPtr == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            return GetMethod(classPtr, methodName, parameters, variations);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to get IL2CPP method pointer: {ex}.");
            return IntPtr.Zero;
        }
    }

    public static void WaitIl2CppInit()
    {
        while (!IL2CPP.il2cpp_is_vm_thread(IntPtr.Zero))
        {
            System.Threading.Thread.Sleep(10);
        }
    }

    public static void AttachIl2Cpp()
    {
        if (IL2CPP.il2cpp_thread_current() == IntPtr.Zero)
        {
            IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
        }
    }

    public static void DetachIl2Cpp()
    {
        var thread = IL2CPP.il2cpp_thread_current();
        if (thread != IntPtr.Zero)
        {
            IL2CPP.il2cpp_thread_detach(thread);
        }
    }
}
