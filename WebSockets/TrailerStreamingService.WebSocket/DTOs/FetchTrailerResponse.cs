﻿using System;
namespace TrailerStreamingService.WebSocket.DTOs
{
    public class FetchTrailerResponse
    {
        public StatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public byte[] Video { get; set; }
    }

}

