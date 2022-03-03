namespace Tiny {
	public static class Json {
		public static string Encode(object value, bool pretty = false) {
			JsonBuilder builder = new JsonBuilder(pretty);
			JsonMapper.EncodeValue(value, builder);
			return builder.ToString();
		}
	}
}

