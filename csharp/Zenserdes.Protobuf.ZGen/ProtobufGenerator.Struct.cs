﻿using Google.Protobuf.Reflection;

using System;
using System.Buffers;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Humanizer;

using Zenserdes.Protobuf.Serialization;
using Zenserdes.Protobuf;
using Zenserdes.Protobuf.ZenGen.Models;

#nullable enable


namespace Zenserdes.Protobuf.ZenGen
{
	public static partial class ProtobufGenerator
	{
		public class Struct
		{
			private readonly IndentedTextWriter _writer;
			private string _package;

			public Struct(IndentedTextWriter writer, string package)
			{
				_writer = writer;
				_package = package;

				_writer.WriteLine("// <auto-generated>");
				_writer.WriteLine("// This code was generated with Zenserdes.Protobuf.ZGen");
				// TODO: figure out cleaner way to specify major version.
				_writer.WriteLine("// The code generated here will only work with Zenserdes.Protobuf 1.*.*");
				_writer.WriteLine("// See the Zenserdes.Protobuf project on github: https://github.com/Zenserdes/protobuf/tree/master/csharp");
				_writer.WriteLine("// </auto-generated>");
			}

			public void Generate(ZProtobufFile proto)
			{
				_writer.WriteLine();

				_writer.WriteLine($"namespace {_package}");
				_writer.WriteLine('{');
				_writer.Indent++;

				var wroteData = 0;

				foreach (var (message, isLast) in proto.Messages.FlagLast())
				{
					wroteData = 1;

					GenerateMessage(message);

					if (!isLast)
					{
						_writer.WriteLine();
					}
				}

				foreach (var (@enum, isLast) in proto.Enums.FlagLast())
				{
					LineHelper(ref wroteData, 2);

					GenerateEnum(@enum);
				}

				_writer.Indent--;
				_writer.WriteLine('}');

				void LineHelper(ref int wroteData, int supposeToBe)
				{
					if (wroteData != supposeToBe)
					{
						if (wroteData > 0)
						{
							_writer.WriteLine();
						}

						wroteData = supposeToBe;
					}
				}
			}

			public void GenerateMessage(ZMessage message)
			{
				var inherits = typeof(IMessageAndOperator<>).FullyQualified(message.FullName);

				_writer.WriteLine($"public partial struct {message.Name} : {inherits}");
				_writer.WriteLine('{');
				_writer.Indent++;

				var wroteData = 0;

				// TODO: condense boilerplate
				foreach (var (field, isLast) in message.Fields.FlagLast())
				{
					wroteData = 1;

					GenerateField(field);

					EndHelper(isLast);
				}

				var areNestedTypes = message.NestedEnums.Any()
					|| message.NestedMessages.Any();

				// nested types like enums & etc
				if (areNestedTypes)
				{
					LineHelper(ref wroteData, 2);

					_writer.WriteLine("public static partial class Types"); // 'Types' to match Google.Protobuf
					_writer.WriteLine('{');
					_writer.Indent++;

					// nested enums
					foreach (var (nestedEnum, isLast) in message.NestedEnums.FlagLast())
					{
						wroteData = 3;

						GenerateEnum(nestedEnum);

						EndHelper(isLast);
					}

					// nested types
					foreach (var (nestedType, isLast) in message.NestedMessages.FlagLast())
					{
						LineHelper(ref wroteData, 4);

						GenerateMessage(nestedType);

						EndHelper(isLast);
					}

					_writer.Indent--;
					_writer.WriteLine('}');
				}

				// and now to implement IMessageAndOperator<TSelf>

				LineHelper(ref wroteData, 5);

				ImplementIMessageAndOperator(message);

				_writer.Indent--;
				_writer.WriteLine('}');

				void LineHelper(ref int wroteData, int supposeToBe)
				{
					if (wroteData != supposeToBe)
					{
						if (wroteData > 0)
						{
							_writer.WriteLine();
						}

						wroteData = supposeToBe;
					}
				}

				void EndHelper(bool isLast)
				{
					if (!isLast)
					{
						_writer.WriteLine();
					}
				}
			}

			public void ImplementIMessageAndOperator(ZMessage message)
			{
				var sizeHint = new SizeHint(_writer);
				sizeHint.Generate(message);
				_writer.WriteLine();

				var serializer = new Serialize(_writer);
				serializer.Generate(message, typeof(SpanScriber));
				_writer.WriteLine();

				serializer.Generate(message, typeof(MemoryScriber));
				_writer.WriteLine();

				serializer.Generate(message, typeof(StreamScriber));
				_writer.WriteLine();

				var exactSize = new ExactSize(_writer);
				exactSize.Generate(message, message.FullName, $"in {message.FullName} message");
				_writer.WriteLine();

				exactSize.Generate(message, message.FullName, $"{message.FullName} message");
				_writer.WriteLine();

				var tryDeserialize = new TryDeserialize(_writer);
				tryDeserialize.Generate(message, message.FullName, typeof(SpanDataStreamer<>));
				_writer.WriteLine();

				tryDeserialize.Generate(message, message.FullName, typeof(MemoryDataStreamer<>));
				_writer.WriteLine();

				tryDeserialize.Generate(message, message.FullName, typeof(StreamDataStreamer<>));
				_writer.WriteLine();
			}

			public void GenerateField(ZField field)
			{
				// TODO: additional information about the field
				_writer.WriteLine("/// <summary>");
				_writer.WriteLine($"/// Index: {field.Index}");
				_writer.WriteLine("/// </summary>");

				if (field.Options.Deprecated == true)
				{
					_writer.WriteLine($"[{typeof(ObsoleteAttribute).FullyQualified()}]");
				}

				_writer.WriteLine($"public {field.CSharpType} {field.FieldName} {{ get; set; }}");
			}

			public void GenerateEnum(ZEnum @enum)
			{
				_writer.WriteLine($"public enum {@enum.Name}");
				_writer.WriteLine('{');
				_writer.Indent++;

				var valuesWritten = false;

				foreach (var (value, isLast) in @enum.Values.FlagLast())
				{
					valuesWritten = true;
					_writer.Write($"{value.Name} = {value.Index}");

					if (!isLast)
					{
						_writer.WriteLine(',');
					}
				}

				if (valuesWritten)
				{
					_writer.WriteLine(); // it's Write within the loop, not WriteLine
				}

				_writer.Indent--;
				_writer.WriteLine('}');
			}

			public string StringTypeOf(FieldDescriptorProto field)
			{
				var type = field.TypeOf();

				if (type == typeof(object))
				{
					var fieldTypeName = field.TypeName;

					// idk if it can ever be null
					if (fieldTypeName == null) throw new NotSupportedException();

					var newTypeName = "." + fieldTypeName.Substring(1).Replace(".", ".Types.");

					return FullyQualifiedProtobuf(newTypeName);
				}

				return type.FullyQualified();
			}

			public string FullyQualifiedProtobuf(string fieldTypeName)
			{
				// we probably have a nested type
				// typeName will have a dot at the beginning, let's get rid of that
				ReadOnlySpan<char> typeName = fieldTypeName;
				typeName = typeName.Slice(1);

				// and stick that onto the namespace
				var strb = new StringBuilder($"global::");

				// we don't ever have to worry about this being null since we always expect a
				// package anyways
				strb.Append(_package);

				strb.Append('.');
				strb.Append(typeName);

				return strb.ToString();
			}
		}
	}

	public static class GeneratorExtensions
	{
		public static Type TypeOf(this FieldDescriptorProto field)
		=> field.type switch
		{
			FieldDescriptorProto.Type.TypeBool => typeof(bool),
			FieldDescriptorProto.Type.TypeSint32 => typeof(int),
			FieldDescriptorProto.Type.TypeInt32 => typeof(int),
			FieldDescriptorProto.Type.TypeUint32 => typeof(uint),
			FieldDescriptorProto.Type.TypeSint64 => typeof(long),
			FieldDescriptorProto.Type.TypeInt64 => typeof(long),
			FieldDescriptorProto.Type.TypeUint64 => typeof(ulong),
			FieldDescriptorProto.Type.TypeDouble => typeof(double),
			FieldDescriptorProto.Type.TypeString => typeof(ReadOnlyMemory<byte>),
			FieldDescriptorProto.Type.TypeBytes => typeof(ReadOnlyMemory<byte>),
			_ => typeof(object)
		};

		// https://github.com/protobuf-net/protobuf-net/blob/master/src/protobuf-net.Reflection/CSharpCodeGenerator.cs#L34
		private static string[] ReservedNames = new string[]
		{
			"abstract", "event", "new", "struct", "as", "explicit", "null", "switch", "base", "extern", "object", "this",
			"bool", "false", "operator", "throw", "break", "finally", "out", "true", "byte", "fixed", "override", "try",
			"case", "float", "params", "typeof", "catch", "for", "private", "uint", "char", "foreach", "protected", "ulong",
			"checked", "goto", "public", "unchecked", "class", "if", "readonly", "unsafe", "const", "implicit", "ref",
			"ushort", "continue", "in", "return", "using", "decimal", "int", "sbyte", "virtual", "default", "interface",
			"sealed", "volatile", "delegate", "internal", "short", "void", "do", "is", "sizeof", "while", "double", "lock",
			"stackall", "else", "long", "static", "enum", "namespace", "string"
		};

		private static string[] UsedMembers = new string[]
		{
			"SizeHint",
			"SizeHint",
			"ExactSize",
			"TryDeserialize"
		};

		// lower: used in enums
		public static string NameOf(this string name, bool lower = false)
		{
			var operatingName = name;

			if (lower)
			{
				operatingName = operatingName.ToLower();
			}

			// don't need to check reserved names because we pascalize it
			operatingName = operatingName.Pascalize();

			if (UsedMembers.Contains(operatingName))
			{
				// aligns with Google.Protobuf generated code
				//
				// e.g. 'Types'
				// we can ensure we don't have to deal with 'operatingName == "Types_"' because
				// that's illegal naming (just try it with protoc)
				operatingName += "_";
			}

			return operatingName;
		}

		private static Dictionary<Type, string> _shortcuts = new Dictionary<Type, string>
		{
			[typeof(sbyte)] = "sbyte",
			[typeof(byte)] = "byte",
			[typeof(short)] = "short",
			[typeof(ushort)] = "ushort",
			[typeof(int)] = "int",
			[typeof(uint)] = "uint",
			[typeof(long)] = "long",
			[typeof(ulong)] = "ulong",
			[typeof(bool)] = "bool",
			[typeof(double)] = "double",
			[typeof(string)] = "string",
			[typeof(object)] = "object",
			[typeof(void)] = "void",
		};

		public static string FullyQualified(this Type type, params string[] genericArgReplacements)
		{
			if (_shortcuts.TryGetValue(type, out var shortcut))
			{
				return shortcut;
			}

			var nameBuilder = new StringBuilder("global::");

			if (type.Namespace != null)
			{
				nameBuilder.Append(type.Namespace);
				nameBuilder.Append('.');
			}

			ReadOnlySpan<char> name = type.Name;
			var backtickIndex = name.IndexOf('`');

			if (backtickIndex >= 0)
			{
				name = name.Slice(0, backtickIndex);
			}

			nameBuilder.Append(name);

			if (type.IsGenericType)
			{
				nameBuilder.Append('<');

				var i = 0;
				foreach (var (generic, isLast) in type.GetGenericArguments().FlagLast())
				{
					if (genericArgReplacements.Length > i)
					{
						nameBuilder.Append(genericArgReplacements[i]);
					}
					else
					{
						nameBuilder.Append(FullyQualified(generic));
					}

					if (!isLast)
					{
						nameBuilder.Append(", ");
					}
					i++;
				}

				nameBuilder.Append('>');
			}

			return nameBuilder.ToString();
		}
	}
}
