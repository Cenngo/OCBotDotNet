﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DiscordNET.Data.Genius
{
	/// <summary>
	/// Complete Search Response for Genius Query
	/// </summary>
	public struct GeniusSearchResponse
	{
		[JsonProperty("meta")]
		public GSMeta Meta { get; set; }
		[JsonProperty("response")]
		public GSResponse Response { get; set; }
	}

	/// <summary>
	/// Meta Data of the Response
	/// </summary>
	public struct GSMeta
	{
		[JsonProperty("status")]
		public int Status { get; set; }
	}
	
	/// <summary>
	/// Data Part of the Genius Query Response
	/// </summary>
	public struct GSResponse
	{
		[JsonProperty("hits")]
		public List<GSHits> Hits { get; set; }
	}

	public struct GSHits
	{
		[JsonProperty("type")]
		public string Type { get; set; }
		[JsonProperty("result")]
		public GSResult Result { get; set; }
	}


	public struct GSResult
	{
		[JsonProperty("full_title")]
		public string FullTitle { get; set; }
		[JsonProperty("header_image_url")]
		public string HeadedImgUrl { get; set; }
		[JsonProperty("id")]
		public int ID { get; set; }
		[JsonProperty("url")]
		public string URL { get; set; }
	}
}
