﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AssEmbly.Resources.Localization {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings_AssemblerWarnings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings_AssemblerWarnings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AssEmbly.Resources.Localization.Strings.AssemblerWarnings", typeof(Strings_AssemblerWarnings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Manually emitted error..
        /// </summary>
        internal static string NonFatal_0000 {
            get {
                return ResourceManager.GetString("NonFatal_0000", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Instruction writes to the rpo register..
        /// </summary>
        internal static string NonFatal_0001 {
            get {
                return ResourceManager.GetString("NonFatal_0001", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Division by constant 0..
        /// </summary>
        internal static string NonFatal_0002 {
            get {
                return ResourceManager.GetString("NonFatal_0002", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File has an entry point explicitly defined, but the program is being assembled into v1 format which doesn&apos;t support them..
        /// </summary>
        internal static string NonFatal_0003 {
            get {
                return ResourceManager.GetString("NonFatal_0003", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Allocating constant 0 bytes..
        /// </summary>
        internal static string NonFatal_0004 {
            get {
                return ResourceManager.GetString("NonFatal_0004", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given literal value does not correspond to a valid terminal colour..
        /// </summary>
        internal static string NonFatal_0005 {
            get {
                return ResourceManager.GetString("NonFatal_0005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Manually emitted suggestion..
        /// </summary>
        internal static string Suggestion_0000 {
            get {
                return ResourceManager.GetString("Suggestion_0000", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Avoid use of NOP instruction..
        /// </summary>
        internal static string Suggestion_0001 {
            get {
                return ResourceManager.GetString("Suggestion_0001", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use the `%PAD` directive instead of chaining `%DAT 0` directives..
        /// </summary>
        internal static string Suggestion_0002 {
            get {
                return ResourceManager.GetString("Suggestion_0002", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Put %IMP directives at the end of the file, unless the position of the directive is important given the file&apos;s contents..
        /// </summary>
        internal static string Suggestion_0003 {
            get {
                return ResourceManager.GetString("Suggestion_0003", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Put data at the end of the file, unless the position of the data is important..
        /// </summary>
        internal static string Suggestion_0004 {
            get {
                return ResourceManager.GetString("Suggestion_0004", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use `TST {1}, {1}` instead of `CMP {1}, 0`, as it results in less bytes..
        /// </summary>
        internal static string Suggestion_0005 {
            get {
                return ResourceManager.GetString("Suggestion_0005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use `XOR {1}, {1}` instead of `{0} {1}, 0`, as it results in less bytes..
        /// </summary>
        internal static string Suggestion_0006 {
            get {
                return ResourceManager.GetString("Suggestion_0006", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use `ICR {1}` instead of `ADD {1}, 1`, as it results in less bytes..
        /// </summary>
        internal static string Suggestion_0007 {
            get {
                return ResourceManager.GetString("Suggestion_0007", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use `DCR {1}` instead of `SUB {1}, 1`, as it results in less bytes..
        /// </summary>
        internal static string Suggestion_0008 {
            get {
                return ResourceManager.GetString("Suggestion_0008", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Operation has no effect..
        /// </summary>
        internal static string Suggestion_0009 {
            get {
                return ResourceManager.GetString("Suggestion_0009", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Shift operation shifts by 64 bits or more, which will always shift out all bits..
        /// </summary>
        internal static string Suggestion_0010 {
            get {
                return ResourceManager.GetString("Suggestion_0010", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove leading 0 digits from denary number..
        /// </summary>
        internal static string Suggestion_0011 {
            get {
                return ResourceManager.GetString("Suggestion_0011", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove useless `%PAD 0` directive..
        /// </summary>
        internal static string Suggestion_0012 {
            get {
                return ResourceManager.GetString("Suggestion_0012", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use `DCR {1}` instead of `ADD {1}, -1`, as it results in less bytes..
        /// </summary>
        internal static string Suggestion_0013 {
            get {
                return ResourceManager.GetString("Suggestion_0013", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use `ICR {1}` instead of `SUB {1}, -1`, as it results in less bytes..
        /// </summary>
        internal static string Suggestion_0014 {
            get {
                return ResourceManager.GetString("Suggestion_0014", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use `MVB {1}, {1}` instead of `AND {1}, 0xFF`, as it results in less bytes..
        /// </summary>
        internal static string Suggestion_0015 {
            get {
                return ResourceManager.GetString("Suggestion_0015", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use `MVW {1}, {1}` instead of `AND {1}, 0xFFFF`, as it results in less bytes..
        /// </summary>
        internal static string Suggestion_0016 {
            get {
                return ResourceManager.GetString("Suggestion_0016", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use `MVD {1}, {1}` instead of `AND {1}, 0xFFFFFFFF`, as it results in less bytes..
        /// </summary>
        internal static string Suggestion_0017 {
            get {
                return ResourceManager.GetString("Suggestion_0017", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Label &quot;{0}&quot; is defined but never used..
        /// </summary>
        internal static string Suggestion_0018 {
            get {
                return ResourceManager.GetString("Suggestion_0018", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to %ASM_ONCE directive is unreachable, as it is not the first one in the file..
        /// </summary>
        internal static string Suggestion_0019 {
            get {
                return ResourceManager.GetString("Suggestion_0019", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use the `HLT` instruction instead of `EXTD_HLT` when the exit code is always 0..
        /// </summary>
        internal static string Suggestion_0020 {
            get {
                return ResourceManager.GetString("Suggestion_0020", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Using the `Q*` pointer size specifier is unnecessary. Use just `*` instead - pointers read 64-bits by default..
        /// </summary>
        internal static string Suggestion_0021 {
            get {
                return ResourceManager.GetString("Suggestion_0021", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Using the `* 1` register multiplier is unnecessary. Register displacements are not multiplied by default..
        /// </summary>
        internal static string Suggestion_0022 {
            get {
                return ResourceManager.GetString("Suggestion_0022", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A displacement constant of 0 has no effect..
        /// </summary>
        internal static string Suggestion_0023 {
            get {
                return ResourceManager.GetString("Suggestion_0023", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use `MVQ` instead of `EXTD_MPA` when the source pointer has no displacement..
        /// </summary>
        internal static string Suggestion_0024 {
            get {
                return ResourceManager.GetString("Suggestion_0024", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Manually emitted warning..
        /// </summary>
        internal static string Warning_0000 {
            get {
                return ResourceManager.GetString("Warning_0000", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data insertion is not directly preceded by an unconditional jump, return, or halt instruction..
        /// </summary>
        internal static string Warning_0001 {
            get {
                return ResourceManager.GetString("Warning_0001", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Jump/Call target address does not point to executable code..
        /// </summary>
        internal static string Warning_0002 {
            get {
                return ResourceManager.GetString("Warning_0002", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Jump/Call target address points to end of file, not executable code..
        /// </summary>
        internal static string Warning_0003 {
            get {
                return ResourceManager.GetString("Warning_0003", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Instruction writes to an address pointing to executable code..
        /// </summary>
        internal static string Warning_0004 {
            get {
                return ResourceManager.GetString("Warning_0004", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Instruction reads from an address pointing to executable code in a context that likely expects data..
        /// </summary>
        internal static string Warning_0005 {
            get {
                return ResourceManager.GetString("Warning_0005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to String insertion is not immediately followed by a 0 (null) byte..
        /// </summary>
        internal static string Warning_0006 {
            get {
                return ResourceManager.GetString("Warning_0006", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Numeric literal is too large for the given move instruction. Upper bits will be truncated at runtime..
        /// </summary>
        internal static string Warning_0007 {
            get {
                return ResourceManager.GetString("Warning_0007", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unreachable code detected..
        /// </summary>
        internal static string Warning_0008 {
            get {
                return ResourceManager.GetString("Warning_0008", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Program runs to end of file without being terminated by an unconditional jump, return, or halt instruction..
        /// </summary>
        internal static string Warning_0009 {
            get {
                return ResourceManager.GetString("Warning_0009", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File import is not directly preceded by an unconditional jump, return, or halt instruction..
        /// </summary>
        internal static string Warning_0010 {
            get {
                return ResourceManager.GetString("Warning_0010", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Instruction writes to the rsf register..
        /// </summary>
        internal static string Warning_0011 {
            get {
                return ResourceManager.GetString("Warning_0011", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Instruction writes to the rsb register..
        /// </summary>
        internal static string Warning_0012 {
            get {
                return ResourceManager.GetString("Warning_0012", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Jump/Call target address points to itself, resulting in an unbreakable infinite loop..
        /// </summary>
        internal static string Warning_0013 {
            get {
                return ResourceManager.GetString("Warning_0013", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unlabelled executable code found after data insertion..
        /// </summary>
        internal static string Warning_0014 {
            get {
                return ResourceManager.GetString("Warning_0014", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Code follows an imported file that is not terminated by unconditional jump, return, or halt instruction..
        /// </summary>
        internal static string Warning_0015 {
            get {
                return ResourceManager.GetString("Warning_0015", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Addresses are 64-bit values, however this move instruction moves less than 64 bits..
        /// </summary>
        internal static string Warning_0016 {
            get {
                return ResourceManager.GetString("Warning_0016", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Entry point does not point to executable code..
        /// </summary>
        internal static string Warning_0017 {
            get {
                return ResourceManager.GetString("Warning_0017", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Entry point points to an import..
        /// </summary>
        internal static string Warning_0018 {
            get {
                return ResourceManager.GetString("Warning_0018", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Signed literal given to an instruction that expects an unsigned literal..
        /// </summary>
        internal static string Warning_0019 {
            get {
                return ResourceManager.GetString("Warning_0019", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Floating point literal given to an instruction that expects an integer literal..
        /// </summary>
        internal static string Warning_0020 {
            get {
                return ResourceManager.GetString("Warning_0020", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Integer literal given to an instruction that expects a floating point literal. Put `.0` at the end of the literal to make it floating point..
        /// </summary>
        internal static string Warning_0021 {
            get {
                return ResourceManager.GetString("Warning_0021", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value is too large for a signed instruction. This positive value will overflow into a negative one..
        /// </summary>
        internal static string Warning_0022 {
            get {
                return ResourceManager.GetString("Warning_0022", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Addresses are unsigned, however this operation is signed..
        /// </summary>
        internal static string Warning_0023 {
            get {
                return ResourceManager.GetString("Warning_0023", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Addresses are integers, however this operation is floating point..
        /// </summary>
        internal static string Warning_0024 {
            get {
                return ResourceManager.GetString("Warning_0024", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use of an extension instruction when assembling to v1 format..
        /// </summary>
        internal static string Warning_0025 {
            get {
                return ResourceManager.GetString("Warning_0025", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to %LABEL_OVERRIDE directive does not have any effect as it is not directly preceded by any label definitions..
        /// </summary>
        internal static string Warning_0026 {
            get {
                return ResourceManager.GetString("Warning_0026", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Addresses are always positive integers, but a negative or floating point literal was given as the label address to the %LABEL_OVERRIDE directive..
        /// </summary>
        internal static string Warning_0027 {
            get {
                return ResourceManager.GetString("Warning_0027", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;@&apos; prefix on the target assembler variable name is not required for this directive. Including it will result in the current value of the directive being used as the target variable name instead..
        /// </summary>
        internal static string Warning_0028 {
            get {
                return ResourceManager.GetString("Warning_0028", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value of assembler variables is always interpreted as an integer, but the provided value is floating point..
        /// </summary>
        internal static string Warning_0029 {
            get {
                return ResourceManager.GetString("Warning_0029", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This assembler variable operation will not work as expected with negative values..
        /// </summary>
        internal static string Warning_0030 {
            get {
                return ResourceManager.GetString("Warning_0030", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Both operands to this comparison are numeric literals, so the result will never change..
        /// </summary>
        internal static string Warning_0031 {
            get {
                return ResourceManager.GetString("Warning_0031", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Explicit pointer size specified in a context that does not read the memory contents of the pointer..
        /// </summary>
        internal static string Warning_0032 {
            get {
                return ResourceManager.GetString("Warning_0032", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pointer size other than 64 bits (`*`/`Q*`) used in a floating point context..
        /// </summary>
        internal static string Warning_0033 {
            get {
                return ResourceManager.GetString("Warning_0033", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Values in displacements are always interpreted as integers, but the provided value is floating point..
        /// </summary>
        internal static string Warning_0034 {
            get {
                return ResourceManager.GetString("Warning_0034", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pointer read size does not match the size of this move instruction. The pointer read size will be ignored..
        /// </summary>
        internal static string Warning_0035 {
            get {
                return ResourceManager.GetString("Warning_0035", resourceCulture);
            }
        }
    }
}
