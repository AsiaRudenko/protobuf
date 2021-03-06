﻿using System;
using System.CodeDom.Compiler;

using Zenserdes.Protobuf.Serialization;
using Zenserdes.Protobuf.ZenGen.Models;

namespace Zenserdes.Protobuf.ZenGen
{
	public static partial class ProtobufGenerator
	{
		public class Serialize
		{
			private IndentedTextWriter _writer;

			public Serialize(IndentedTextWriter writer) => _writer = writer;

			public void Generate(ZMessage message, Type type)
			{
				var returnType = type == typeof(StreamScriber) ? typeof(void) : typeof(bool);
				_writer.WriteMethod(returnType, typeof(IMessage), null, nameof(IMessage.Serialize), () =>
				{
					_writer.WriteLine("throw new " + typeof(NotImplementedException).FullyQualified() + "();");
				}, $"{type.FullyQualified()} target");
			}
		}
	}
}