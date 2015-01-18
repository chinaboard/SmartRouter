// Copyright (c) 2014 Matthias Specht
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotArgs
{
    /// <summary>Argument that acts as an alias for another argument.</summary>
    public class AliasArgument : Argument
    {
        /// <summary>Initializes a new instance of the <see cref="AliasArgument"/> class.</summary>
        /// <param name="entry">The argument this alias should mirror.</param>
        public AliasArgument(Argument entry)
            : base(entry.DefaultValue, entry.IsRequired)
        {
            Reference = entry;
        }

        /// <summary>Gets the value of this argument.</summary>
        /// <returns>The argument's value.</returns>
        public override object GetValue()
        {
            return Reference.GetValue();
        }

        /// <summary>Sets the value for this argument.</summary>
        /// <param name="value">The value to set.</param>
        public override void SetValue(object value)
        {
            Reference.SetValue(value);
        }

        /// <summary>Validates the specified value.</summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> is valid; otherwise <c>false</c> .
        /// </returns>
        protected internal override bool Validate(object value)
        {
            return Reference.Validate(value);
        }

        /// <summary>
        /// The default value that will be used if no value was passed on the command line.
        /// </summary>
        /// <remarks>Using this when <see cref="IsRequired"/> is set will have no effect.</remarks>
        public new object DefaultValue
        {
            get { return Reference.DefaultValue; }
            protected internal set { Reference.DefaultValue = value; }
        }

        /// <summary>
        /// Flag indicating whether this argument is required, i.e. must be provided via the command line.
        /// </summary>
        public new bool IsRequired { get { return Reference.IsRequired; } }

        /// <summary>Position this argument is expected to be located in the command line.</summary>
        public new int? Position { get { return Reference.Position; } }

        /// <summary>
        /// Flag indicating whether multplie calls to <see cref="SetValue" /> will add a value or overwrite the existing one.
        /// </summary>
        public new bool SupportsMultipleValues { get { return Reference.SupportsMultipleValues; } }

        private Argument Reference;
    }

    /// <summary>
    /// Base class for an argument that can be registered with a <see cref="CommandLineArgs"/> .
    /// </summary>
    public abstract class Argument
    {
        /// <summary>Initializes a new instance of the <see cref="Argument"/> class.</summary>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="required">Flag indicating whether this argument is required.</param>
        /// <param name="position">Position this argument is expected to be located in the command line.</param>
        public Argument(object defaultValue, bool required = false, int? position = null)
        {
            DefaultValue = defaultValue;
            IsRequired = required;
            SupportsMultipleValues = false;
            Position = position;

            if (IsRequired)
            {
                DefaultValue = null;
            }
        }

        /// <summary>Gets the value of this argument.</summary>
        /// <returns>The argument's value.</returns>
        public virtual object GetValue()
        {
            return Value;
        }

        /// <summary>Resets this argument.</summary>
        public virtual void Reset()
        {
            Value = DefaultValue;
        }

        /// <summary>Sets the value for this argument.</summary>
        /// <param name="value">The value to set.</param>
        public virtual void SetValue(object value)
        {
            Value = value;
        }

        /// <summary>Validates the specified value.</summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> is valid; otherwise <c>false</c> .
        /// </returns>
        protected internal abstract bool Validate(object value);

        /// <summary>
        /// The default value that will be used if no value was passed on the command line.
        /// </summary>
        /// <remarks>Using this when <see cref="IsRequired"/> is set will have no effect.</remarks>
        public object DefaultValue { get; protected internal set; }

        /// <summary>The message that will be displayed in the help page for your program.</summary>
        public string HelpMessage { get; set; }

        /// <summary>
        /// Value that will be shown (in upper case) in the usage page for this argument. Setting
        /// this to <c>null</c> will display the default value (i.e. OPTION, COLLECTION, etc.).
        /// </summary>
        public string HelpPlaceholder { get; set; }

        /// <summary>
        /// Flag indicating whether this argument is required, i.e. must be provided via the command line.
        /// </summary>
        public bool IsRequired { get; protected set; }

        /// <summary>Indicates whether this argument requires an explicit option.</summary>
        public bool NeedsValue { get; protected set; }

        /// <summary>Position this argument is expected to be located in the command line.</summary>
        public int? Position { get; set; }

        /// <summary>A method that can be executed when the command line arguments are processed.</summary>
        public Action<object> Processor { get; set; }

        /// <summary>
        /// Flag indicating whether multplie calls to <see cref="SetValue"/> will add a value or overwrite the existing one.
        /// </summary>
        public bool SupportsMultipleValues { get; protected set; }

        /// <summary>A method that can be used to validate a value for this argument.</summary>
        public Func<object, bool> Validator { get; set; }

        private object Value;
    }

    /// <summary>An option that can take multiple values.</summary>
    public class CollectionArgument : OptionArgument
    {
        /// <summary>Initializes a new instance of the <see cref="CollectionArgument"/> class.</summary>
        /// <param name="required">Flag indicating whether this argument is required.</param>
        /// <param name="position">Position this argument is expected to be located in the command line.</param>
        public CollectionArgument(bool required = false, int? position = null)
            : base(null, required, position)
        {
            SupportsMultipleValues = true;
            HelpPlaceholder = "COLLECTION";
            base.SetValue(new string[0]);
        }

        /// <summary>Gets the value of this argument.</summary>
        /// <returns>The argument's value.</returns>
        public override object GetValue()
        {
            return Values.ToArray();
        }

        /// <summary>Resets this argument.</summary>
        public override void Reset()
        {
            Values.Clear();
        }

        /// <summary>Sets the value for this argument.</summary>
        /// <param name="value">The value to set.</param>
        public override void SetValue(object value)
        {
            Values.Add(value as string);
        }

        private List<string> Values = new List<string>();
    }

    /// <summary>Class for defining, validating and processing command line arguments.</summary>
    public class CommandLineArgs
    {
        /// <summary>Initializes a new instance of the <see cref="CommandLineArgs"/> class.</summary>
        public CommandLineArgs()
        {
            OutputWriter = Console.Out;
            ExecuteableName = Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().Location);
        }

        /// <summary>Adds an example that will be displayed on the help page.</summary>
        /// <param name="description">The name or description for this example.</param>
        /// <param name="commandLine">The command line to display for this example.</param>
        public void AddExample(string description, string commandLine)
        {
            Examples.Add(description, commandLine);
        }

        /// <summary>Gets the value of an argument.</summary>
        /// <param name="name">Name of the argument to read.</param>
        /// <returns>
        /// The effective value of the argument. If the argument was omitted in the arguments, the
        /// default value will be returned.
        /// </returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        /// An argument with the name <paramref name="name"/> was not registered.
        /// </exception>
        public T GetValue<T>(string name)
        {
            if (!Arguments.ContainsKey(name))
            {
                throw new KeyNotFoundException(string.Format("An collection with the name {0} was not registered.", name));
            }

            Argument entry = Arguments[name];
            return (T)entry.GetValue();
        }

        /// <summary>Prints a help message describing the effects of all available options.</summary>
        /// <param name="errorMessage">Optional error message to display.</param>
        public void PrintHelp(string errorMessage = null)
        {
            string argList = string.Join(" ", Arguments.OrderBy(k => k.Key).Select(a => ArgumentToArgList(a.Key, a.Value)));

            OutputWriter.WriteLine(ApplicationInfo);
            OutputWriter.WriteLine();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                OutputWriter.WriteLine(errorMessage);
                OutputWriter.WriteLine();
            }
            OutputWriter.WriteLine("Usage:");
            OutputWriter.WriteLine("{0} {1}", ExecuteableName, argList);

            foreach (KeyValuePair<string, Argument> kvp in Arguments.OrderBy(k => k.Key))
            {
                OutputWriter.WriteLine();
                OutputWriter.WriteLine("{0,-10}{1}", kvp.Key, kvp.Value.HelpMessage);
                OutputWriter.WriteLine("{0,-10}{1}", "", GetArgumentInfo(kvp.Value));
            }

            if (Examples.Any())
            {
                OutputWriter.WriteLine();
                OutputWriter.WriteLine("Examples:");

                foreach (KeyValuePair<string, string> kvp in Examples.OrderBy(k => k.Key))
                {
                    OutputWriter.WriteLine();
                    OutputWriter.WriteLine(kvp.Key);
                    OutputWriter.WriteLine(kvp.Value);
                }
            }
        }

        /// <summary>
        /// Processes all registered arguments that have their <see cref="Argument.Processor"/> set.
        /// </summary>
        public void Process()
        {
            foreach (Argument arg in Arguments.Values.Where(a => !(a is AliasArgument)))
            {
                if (arg.Processor != null)
                {
                    arg.Processor(arg.GetValue());
                }
            }
        }

        /// <summary>Registers an alias for an existing entry.</summary>
        /// <param name="originalName">Name of the original option.</param>
        /// <param name="alias">The alias to add for the option.</param>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        /// An entry with the name <paramref name="originalName"/> was not registered.
        /// </exception>
        public void RegisterAlias(string originalName, string alias)
        {
            if (!Arguments.ContainsKey(originalName))
            {
                throw new KeyNotFoundException(string.Format("An entry with the name {0} was not registered.", originalName));
            }

            AliasArgument entry = new AliasArgument(Arguments[originalName]);
            Arguments[alias] = entry;
        }

        /// <summary>Registers a new argument.</summary>
        /// <param name="name">Name of the argument to register.</param>
        /// <param name="arg">The argument's configuration.</param>
        public void RegisterArgument(string name, Argument arg)
        {
            Arguments[name] = arg;
        }

        /// <summary>
        /// Registers a help argument that will display the help page for the program if set by the user.
        /// </summary>
        /// <param name="name">Name of the flag. The default value is "help".</param>
        public void RegisterHelpArgument(string name = "help")
        {
            FlagArgument arg = new FlagArgument();
            arg.Processor = (v) => PrintHelp();
            arg.HelpMessage = "Displays this help.";

            RegisterArgument(name, arg);
        }

        /// <summary>
        /// Sets the default argument that will be filled when no argument name is given.
        /// </summary>
        /// <param name="argument">Name of the argument to use as the default.</param>
        public void SetDefaultArgument(string argument)
        {
            if (!Arguments.ContainsKey(argument))
            {
                throw new ArgumentException(string.Format("Argument {0} was not registered", argument), "argument");
            }

            DefaultArgument = argument;
        }

        /// <summary>Processes a set of command line arguments.</summary>
        /// <param name="args">
        /// Command line arguments to process. This is usally coming from your Main method.
        /// </param>
        /// <param name="outErrors">
        /// Optional "out" parameter that holds error strings for every encountered error.
        /// </param>
        /// <returns>
        /// <c>true</c> if the arguments in <paramref name="args"/> are valid; otherwise
        /// <c>false</c> .
        /// </returns>
        public bool Validate(string[] args, OptionalOut<string[]> outErrors = null)
        {
            return Validate(string.Join(" ", args), outErrors);
        }

        /// <summary>Processes a set of command line arguments.</summary>
        /// <param name="args">
        /// Command line arguments to process. This is usally coming from your Main method.
        /// </param>
        /// <param name="outErrors">
        /// Optional "out" parameter that holds error strings for every encountered error.
        /// </param>
        /// <returns>
        /// <c>true</c> if the arguments in <paramref name="args"/> are valid; otherwise
        /// <c>false</c> .
        /// </returns>
        public bool Validate(string args, OptionalOut<string[]> outErrors = null)
        {
            Reset();

            bool ignoreAlreadyHandled = false;
            if (DefaultArgument != null)
            {
                ignoreAlreadyHandled = Arguments[DefaultArgument].SupportsMultipleValues;
            }

            bool handledDefault = false;
            bool errors = false;
            List<string> errorList = new List<string>();

            List<string> parts = SplitCommandLine(args);
            for (int i = 0; i < parts.Count; ++i)
            {
                string arg = GetArgName(parts[i]);
                if (!IsArgumentName(parts[i]))
                {
                    Argument posArgument = GetArgumentForPosition(i);
                    if (posArgument != null)
                    {
                        string argName = GetNameForArgument(posArgument);

                        parts[i] = string.Format("/{0}={1}", argName, arg);
                        arg = argName;
                    }
                    else if (DefaultArgument != null)
                    {
                        if (!handledDefault || ignoreAlreadyHandled)
                        {
                            parts[i] = string.Format("/{0}={1}", DefaultArgument, arg);
                            arg = DefaultArgument;

                            handledDefault = true;
                        }
                    }
                }

                if (!Arguments.ContainsKey(arg))
                {
                    if (DefaultArgument != null && (!handledDefault || ignoreAlreadyHandled))
                    {
                        parts[i] = string.Format("/{0}={1}", DefaultArgument, arg);
                        arg = DefaultArgument;

                        handledDefault = true;
                    }
                    else
                    {
                        errorList.Add(string.Format("Unknown option: '{0}'", arg));

                        errors = true;
                        continue;
                    }
                }

                Argument entry = Arguments[arg];

                if (entry.NeedsValue)
                {
                    // Not so simple cases: Collection and Option
                    string value = ExtractValueFromArg(parts[i]);

                    if (value == null && i < parts.Count - 1)
                    {
                        value = parts[i + 1];

                        if (Arguments.ContainsKey(GetArgName(value)))
                        {
                            value = null;
                        }
                        else
                        {
                            i++;
                        }
                    }

                    if (value != null)
                    {
                        entry.SetValue(value);
                    }
                    else
                    {
                        // Missing argument
                        errorList.Add(string.Format("Missing value for option '{0}'", arg));
                        errors = true;
                    }
                }
                else // Simple case: a flag
                {
                    entry.SetValue(true);
                }
            }

            foreach (KeyValuePair<string, Argument> kvp in Arguments)
            {
                Argument entry = kvp.Value;
                object value = entry.GetValue();

                if (entry.IsRequired && value == null)
                {
                    errorList.Add(string.Format("Missing value for option '{0}'", kvp.Key));
                    errors = true;
                }

                if (!entry.Validate(value))
                {
                    errorList.Add(string.Format("{0}: Invalid value {1}", kvp.Key, value));
                    errors = true;
                }
            }

            if (outErrors != null)
            {
                outErrors.Result = errorList.Distinct().ToArray();
            }

            return !errors;
        }

        private string ArgumentToArgList(string name, Argument arg)
        {
            string desc = string.Format("/{0}", name);
            if (arg.NeedsValue)
            {
                desc += string.Format("={0}", arg.HelpPlaceholder);
            }

            if (arg.IsRequired)
            {
                return string.Format("<{0}>", desc);
            }
            else
            {
                if (arg.DefaultValue != null)
                {
                    desc += string.Format(", {0}", arg.DefaultValue);
                }

                return string.Format("[{0}]", desc);
            }
        }

        private string ExtractValueFromArg(string arg)
        {
            char[] seperators = new[] { '=', ':' };

            int idx = arg.IndexOfAny(seperators);
            if (idx == -1)
            {
                return null;
            }

            return arg.Substring(idx + 1);
        }

        private string GetArgName(string arg)
        {
            char[] seperators = new[] { '=', ':' };
            char[] prefixes = new[] { '-', '/' };

            int end = arg.Length;
            int remove = 0;
            bool atStart = true;

            for (int i = 0; i < arg.Length; ++i)
            {
                if (prefixes.Contains(arg[i]) && atStart)
                {
                    remove++;
                }
                else if (seperators.Contains(arg[i]))
                {
                    end = i;
                }
                else
                {
                    atStart = false;
                }
            }

            if (remove > 0)
            {
                arg = arg.Substring(remove);
                end -= remove;
            }

            return arg.Substring(0, end);
        }

        private Argument GetArgumentForPosition(int position)
        {
            return Arguments.Values.FirstOrDefault(a => a.Position.HasValue && a.Position.Value == position);
        }

        private string GetArgumentInfo(Argument arg)
        {
            string str = "";

            if (arg.IsRequired)
            {
                str = "Required";
            }
            else
            {
                str = "Optional";

                if (arg.DefaultValue != null)
                {
                    str += string.Format(", Default value: {0}", arg.DefaultValue);
                }
            }

            return str;
        }

        private string GetNameForArgument(Argument arg)
        {
            foreach (var kvp in Arguments)
            {
                if (kvp.Value == arg)
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        private bool IsArgumentName(string arg)
        {
            char[] prefixes = new[] { '-', '/' };

            foreach (char p in prefixes)
            {
                if (arg.StartsWith(p.ToString()))
                {
                    return true;
                }
            }

            return false;
        }

        private void Reset()
        {
            foreach (Argument entry in Arguments.Values)
            {
                entry.Reset();
            }
        }

        private List<string> SplitCommandLine(string args)
        {
            List<string> parts = new List<string>();

            string buffer = string.Empty;
            bool inDoubleString = false;
            bool inSingleString = false;

            foreach (char c in args)
            {
                if (c == '\'')
                {
                    if (!inDoubleString)
                    {
                        inSingleString = !inSingleString;
                    }
                    else
                    {
                        buffer += c;
                    }
                }
                else if (c == '"')
                {
                    if (!inSingleString)
                    {
                        inDoubleString = !inDoubleString;
                    }
                    else
                    {
                        buffer += c;
                    }
                }
                else if (c == ' ')
                {
                    if (!inDoubleString && !inSingleString)
                    {
                        if (!string.IsNullOrWhiteSpace(buffer))
                        {
                            parts.Add(buffer);
                        }
                        buffer = string.Empty;
                    }
                    else
                    {
                        buffer += c;
                    }
                }
                else
                {
                    buffer += c;
                }
            }

            if (!string.IsNullOrWhiteSpace(buffer))
            {
                parts.Add(buffer);
            }

            return parts;
        }

        /// <summary>Information about the application that will be displayed in the usage page.</summary>
        /// <example>MyCoolProgram v1.2 Copyright (C) John Smith &lt;smith@example.com&gt;</example>
        public string ApplicationInfo { get; set; }

        /// <summary>Name of the executeable that will be displayed in the usage page.</summary>
        /// <remarks>
        /// The default value for this is the name of the assembly containing the code that created
        /// this object.
        /// </remarks>
        public string ExecuteableName { get; set; }

        /// <summary>
        /// The TextWriter that is used to write the output. The default value is to use <see cref="Console.Out"/>
        /// </summary>
        public TextWriter OutputWriter { get; set; }

        private Dictionary<string, Argument> Arguments = new Dictionary<string, Argument>();
        private string DefaultArgument = null;
        private Dictionary<string, string> Examples = new Dictionary<string, string>();
    }

    /// <summary>A simple argument flag.</summary>
    /// <remarks>
    /// A flag can be specified by passing <c>-FLAG</c> , <c>--flag</c> or <c>/FLAG</c> from the
    /// command line.
    /// </remarks>
    public class FlagArgument : Argument
    {
        /// <summary>Initializes a new instance of the <see cref="FlagArgument"/> class.</summary>
        /// <param name="defaultValue">The default value for this flag.</param>
        /// <param name="required">Flag indicating whether this argument is required.</param>
        /// <param name="position">Position this argument is expected to be located in the command line.</param>
        public FlagArgument(bool defaultValue = false, bool required = false, int? position = null)
            : base(defaultValue, required, position)
        {
            if (IsRequired)
            {
                DefaultValue = null;
            }
        }

        /// <summary>Validates the specified value.</summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> is valid; otherwise <c>false</c> .
        /// </returns>
        protected internal override bool Validate(object value)
        {
            bool valid = value is bool;

            if (IsRequired)
            {
                valid = valid && value != null;
            }

            return valid;
        }
    }

    /// <summary>
    /// Helper class that offers a somewhat elegant solution for the problem of wanting to have an
    /// "optional out" parameter for a method.
    /// </summary>
    /// <typeparam name="TResult">Type of the out parameter.</typeparam>
    public class OptionalOut<TResult>
    {
        /// <summary>The actual value of the out parameter.</summary>
        public TResult Result { get; set; }
    }

    /// <summary>An argument that can have any value.</summary>
    /// <remarks>
    /// An option can be specified by passing <c>-OPTION VALUE</c> , <c>-OPTION=VALUE</c> ,
    /// <c>-OPTION:VALUE</c> , <c>--OPTION VALUE</c> , <c>--OPTION=VALUE</c> , <c>--OPTION:VALUE</c>
    /// , <c>/OPTION VALUE</c> , <c>/OPTION:VALUE</c> or <c>/OPTION=VALUE</c> from the command line
    /// </remarks>
    public class OptionArgument : Argument
    {
        /// <summary>Initializes a new instance of the <see cref="OptionArgument"/> class.</summary>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="required">Flag indicating whether this argument is required.</param>
        /// <param name="position">Position this argument is expected to be located in the command line.</param>
        public OptionArgument(string defaultValue, bool required = false, int? position = null)
            : base(defaultValue, required, position)
        {
            HelpPlaceholder = "OPTION";
            NeedsValue = true;
        }

        /// <summary>Validates the specified value.</summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> is valid; otherwise <c>false</c> .
        /// </returns>
        protected internal override bool Validate(object value)
        {
            if (Validator != null)
            {
                return Validator(value);
            }

            return true;
        }
    }

    /// <summary>A set argument is an option that only takes values from a predefined list.</summary>
    public class SetArgument : OptionArgument
    {
        /// <summary>Initializes a new instance of the <see cref="SetArgument"/> class.</summary>
        /// <param name="validOptions">The valid options this argument may be given.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="required">Flag indicating whether this argument is required.</param>
        /// <param name="position">Position this argument is expected to be located in the command line.</param>
        public SetArgument(string[] validOptions, string defaultValue, bool required = false, int? position = null)
            : base(defaultValue, required, position)
        {
            ValidOptions = validOptions;
        }

        /// <summary>Validates the specified value.</summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> is valid; otherwise <c>false</c> .
        /// </returns>
        protected internal override bool Validate(object value)
        {
            return ValidOptions.Contains(value as string);
        }

        private string[] ValidOptions;
    }
}