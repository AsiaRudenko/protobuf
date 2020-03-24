using Google.Protobuf.Reflection;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace Zenserdes.Protobuf.ZGen
{
	public static partial class ProtobufGenerator
	{
		public class ExactSize
		{
			private IndentedTextWriter _writer;

			public ExactSize(IndentedTextWriter writer) => _writer = writer;

			public void Generate(DescriptorProto message, string fullyQualifiedMessageName, string parameter)
			{
				_writer.WriteMethod(typeof(ulong), typeof(IMessageOperator<>), new string[] { fullyQualifiedMessageName }, nameof(IMessageAndOperator<IWantToUseNameof>.ExactSize), () =>
				{
					_writer.WriteLine("throw new " + typeof(NotImplementedException).FullyQualified() + "();");
				}, parameter);
			}
		}
	}
}
