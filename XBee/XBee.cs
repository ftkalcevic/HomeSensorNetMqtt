#define SERIES_1
#define SERIES_2

/* Ported to C# by Frank Tkalcevic
 */

/**
 * Copyright (c) 2009 Andrew Rapp. All rights reserved.
 *
 * This file is part of XBee-Arduino.
 *
 * XBee-Arduino is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * XBee-Arduino is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with XBee-Arduino.  If not, see <http://www.gnu.org/licenses/>.
 */


using System;
using System.IO.Ports;
using System.Diagnostics;

namespace XBeeLib
{
	public static class Extensions
	{
		/// <summary>
		/// Get the array slice between the two indexes.
		/// ... Inclusive for start index, exclusive for end index.
		/// https://www.dotnetperls.com/array-slice
		/// </summary>
		public static T[] Slice<T>(this T[] source, int start, int end)
		{
			// Handles negative ends.
			if (end < 0)
			{
				end = source.Length + end;
			}
			int len = end - start;

			// Return new array.
			T[] res = new T[len];
			for (int i = 0; i < len; i++)
			{
				res[i] = source[i + start];
			}
			return res;
		}
	}

	/**
	 * The super class of all XBee responses (RX packets)
	 * Users should never attempt to create an instance of this class; instead
	 * create an instance of a subclass
	 * It is recommend to reuse subclasses to conserve memory
	 */
	public class XBeeResponse
	{
		/**
		 * Returns Api Id of the response
		 */
		public byte getApiId()
		{
			return _apiId;
		}
		public void setApiId(byte apiId)
		{
			_apiId = apiId;
		}

		/**
		 * Returns the MSB length of the packet
		 */
		public byte getMsbLength()
		{
			return _msbLength;
		}
		public void setMsbLength(byte msbLength)
		{
			_msbLength = msbLength;
		}

		/**
		 * Returns the LSB length of the packet
		 */
		public byte getLsbLength()
		{
			return _lsbLength;
		}

		public void setLsbLength(byte lsbLength)
		{
			_lsbLength = lsbLength;
		}

		/**
		 * Returns the packet checksum
		 */
		public byte getChecksum()
		{
			return _checksum;
		}

		public void setChecksum(byte checksum)
		{
			_checksum = checksum;
		}

		/**
		 * Returns the length of the frame data: all bytes after the api id, and prior to the checksum
		 * Note up to release 0.1.2, this was incorrectly including the checksum in the length.
		 */
		public byte getFrameDataLength()
		{
			return _frameLength;
		}
		public void setFrameData(byte [] frameDataPtr)
		{
			_frameDataPtr = frameDataPtr;
		}

		/**
		 * Returns the buffer that contains the response.
		 * Starts with byte that follows API ID and includes all bytes prior to the checksum
		 * Length is specified by getFrameDataLength()
		 * Note: Unlike Digi's definition of the frame data, this does not start with the API ID..
		 * The reason for this is all responses include an API ID, whereas my frame data
		 * includes only the API specific data.
		 */
		public byte[] getFrameData()
		{
			return _frameDataPtr;
		}


		public void setFrameLength(byte frameLength)
		{
			_frameLength = frameLength;
		}

		// to support future 65535 byte packets I guess
		/**
		 * Returns the length of the packet
		 */
		public UInt16 getPacketLength()
		{
			return (UInt16)(((_msbLength << 8) & 0xff) + (_lsbLength & 0xff));
		}

		/**
		 * Resets the response to default values
		 */

		public void reset()
		{
			init();
			_apiId = 0;
			_msbLength = 0;
			_lsbLength = 0;
			_checksum = 0;
			_frameLength = 0;

			_errorCode = XBee.NO_ERROR;
		}

		/**
		 * Initializes the response
		 */
		public void init()
		{
			_complete = false;
			_errorCode = XBee.NO_ERROR;
			_checksum = 0;
		}

#if SERIES_2
		/**
		 * Call with instance of ZBTxStatusResponse class only if getApiId() == ZB_TX_STATUS_RESPONSE
		 * to populate response
		 */
		public void getZBTxStatusResponse(XBeeResponse zbXBeeResponse)
		{

			// way off?
			ZBTxStatusResponse zb = (ZBTxStatusResponse)(zbXBeeResponse);
			// pass pointer array to subclass
			zb.setFrameData(getFrameData());
			setCommon(zbXBeeResponse);
		}


		/**
		 * Call with instance of ZBRxResponse class only if getApiId() == ZB_RX_RESPONSE
		 * to populate response
		 */
		public void getZBRxResponse(XBeeResponse rxResponse)
		{

			ZBRxResponse zb = (ZBRxResponse)(rxResponse);

			//TODO verify response api id matches this api for this response

			// pass pointer array to subclass
			zb.setFrameData(getFrameData());
			setCommon(rxResponse);

			zb.getRemoteAddress64().setMsb((UInt32)(((UInt32)(getFrameData()[0]) << 24) + ((UInt32)(getFrameData()[1]) << 16) + ((UInt16)(getFrameData()[2]) << 8) + getFrameData()[3]));
			zb.getRemoteAddress64().setLsb((UInt32)(((UInt32)(getFrameData()[4]) << 24) + ((UInt32)(getFrameData()[5]) << 16) + ((UInt16)(getFrameData()[6]) << 8) + (getFrameData()[7])));
		}

		/**
		 * Call with instance of ZBExplicitRxResponse class only if getApiId() == ZB_EXPLICIT_RX_RESPONSE
		 * to populate response
		 */
		public void getZBExplicitRxResponse(XBeeResponse rxResponse)
		{
			// Nothing to add to that
			getZBRxResponse(rxResponse);
		}

		/**
		 * Call with instance of ZBRxIoSampleResponse class only if getApiId() == ZB_IO_SAMPLE_RESPONSE
		 * to populate response
		 */
		public void getZBRxIoSampleResponse(XBeeResponse response)
		{
			ZBRxIoSampleResponse zb = (ZBRxIoSampleResponse)(response);


			// pass pointer array to subclass
			zb.setFrameData(getFrameData());
			setCommon(response);

			zb.getRemoteAddress64().setMsb((UInt32)(((UInt32)(getFrameData()[0]) << 24) + ((UInt32)(getFrameData()[1]) << 16) + ((UInt16)(getFrameData()[2]) << 8) + getFrameData()[3]));
			zb.getRemoteAddress64().setLsb((UInt32)(((UInt32)(getFrameData()[4]) << 24) + ((UInt32)(getFrameData()[5]) << 16) + ((UInt16)(getFrameData()[6]) << 8) + (getFrameData()[7])));
		}
#endif

#if SERIES_1
		/**
		 * Call with instance of TxStatusResponse only if getApiId() == TX_STATUS_RESPONSE
		 */
		public void getTxStatusResponse(XBeeResponse txResponse)
		{

			TxStatusResponse txStatus = (TxStatusResponse)(txResponse);

			// pass pointer array to subclass
			txStatus.setFrameData(getFrameData());
			setCommon(txResponse);
		}


		/**
		 * Call with instance of Rx16Response only if getApiId() == RX_16_RESPONSE
		 */
		public void getRx16Response(XBeeResponse rx16Response)
		{

			Rx16Response rx16 = (Rx16Response)(rx16Response);

			// pass pointer array to subclass
			rx16.setFrameData(getFrameData());
			setCommon(rx16Response);
		}

		/**
		 * Call with instance of Rx64Response only if getApiId() == RX_64_RESPONSE
		 */
		public void getRx64Response(XBeeResponse rx64Response)
		{

			Rx64Response rx64 = (Rx64Response)(rx64Response);

			// pass pointer array to subclass
			rx64.setFrameData(getFrameData());
			setCommon(rx64Response);

			rx64.getRemoteAddress64().setMsb((UInt32)(((UInt32)(getFrameData()[0]) << 24) + ((UInt32)(getFrameData()[1]) << 16) + ((UInt16)(getFrameData()[2]) << 8) + getFrameData()[3]));
			rx64.getRemoteAddress64().setLsb((UInt32)(((UInt32)(getFrameData()[4]) << 24) + ((UInt32)(getFrameData()[5]) << 16) + ((UInt16)(getFrameData()[6]) << 8) + getFrameData()[7]));
		}

		/**
		 * Call with instance of Rx16IoSampleResponse only if getApiId() == RX_16_IO_RESPONSE
		 */
		public void getRx16IoSampleResponse(XBeeResponse response)
		{
			Rx16IoSampleResponse rx = (Rx16IoSampleResponse)(response);

			rx.setFrameData(getFrameData());
			setCommon(response);
		}

		/**
		 * Call with instance of Rx64IoSampleResponse only if getApiId() == RX_64_IO_RESPONSE
		 */
		public void getRx64IoSampleResponse(XBeeResponse response)
		{
			Rx64IoSampleResponse rx = (Rx64IoSampleResponse)(response);

			rx.setFrameData(getFrameData());
			setCommon(response);

			rx.getRemoteAddress64().setMsb((UInt32)(((UInt32)(getFrameData()[0]) << 24) + ((UInt32)(getFrameData()[1]) << 16) + ((UInt16)(getFrameData()[2]) << 8) + getFrameData()[3]));
			rx.getRemoteAddress64().setLsb((UInt32)(((UInt32)(getFrameData()[4]) << 24) + ((UInt32)(getFrameData()[5]) << 16) + ((UInt16)(getFrameData()[6]) << 8) + getFrameData()[7]));
		}

#endif

		/**
		 * Call with instance of AtCommandResponse only if getApiId() == AT_COMMAND_RESPONSE
		 */
		public void getAtCommandResponse(XBeeResponse atCommandResponse)
		{
			AtCommandResponse at = (AtCommandResponse)(atCommandResponse);

			// pass pointer array to subclass
			at.setFrameData(getFrameData());
			setCommon(atCommandResponse);
		}

		/**
		 * Call with instance of RemoteAtCommandResponse only if getApiId() == REMOTE_AT_COMMAND_RESPONSE
		 */
		public void getRemoteAtCommandResponse(XBeeResponse response)
		{

			// TODO no real need to cast.  change arg to match expected class
			RemoteAtCommandResponse at = (RemoteAtCommandResponse)(response);

			// pass pointer array to subclass
			at.setFrameData(getFrameData());
			setCommon(response);

			at.getRemoteAddress64().setMsb((UInt32)(((UInt32)(getFrameData()[1]) << 24) + ((UInt32)(getFrameData()[2]) << 16) + ((UInt16)(getFrameData()[3]) << 8) + getFrameData()[4]));
			at.getRemoteAddress64().setLsb((UInt32)(((UInt32)(getFrameData()[5]) << 24) + ((UInt32)(getFrameData()[6]) << 16) + ((UInt16)(getFrameData()[7]) << 8) + (getFrameData()[8])));

		}


		/**
		 * Call with instance of ModemStatusResponse only if getApiId() == MODEM_STATUS_RESPONSE
		 */
		public void getModemStatusResponse(XBeeResponse modemStatusResponse)
		{

			ModemStatusResponse modem = (ModemStatusResponse)(modemStatusResponse);

			// pass pointer array to subclass
			modem.setFrameData(getFrameData());
			setCommon(modemStatusResponse);

		}

		/**
		 * Returns true if the response has been successfully parsed and is complete and ready for use
		 */
		public bool isAvailable()
		{
			return _complete;
		}

		public void setAvailable(bool complete)
		{
			_complete = complete;
		}

		/**
		 * Returns true if the response contains errors
		 */
		public bool isError()
		{
			return _errorCode > 0;
		}

		/**
		 * Returns an error code, or zero, if successful.
		 * Error codes include: CHECKSUM_FAILURE, PACKET_EXCEEDS_BYTE_ARRAY_LENGTH, UNEXPECTED_START_BYTE
		 */
		public byte getErrorCode()
		{
			return _errorCode;
		}

		public void setErrorCode(byte errorCode)
		{
			_errorCode = errorCode;
		}


		// pointer to frameData
		protected byte [] _frameDataPtr;

		// copy common fields from xbee response to target response
		private void setCommon(XBeeResponse target)
		{
			target.setApiId(getApiId());
			target.setAvailable(isAvailable());
			target.setChecksum(getChecksum());
			target.setErrorCode(getErrorCode());
			target.setFrameLength(getFrameDataLength());
			target.setMsbLength(getMsbLength());
			target.setLsbLength(getLsbLength());
		}

		private byte _apiId;
	    private byte _msbLength;
	    private byte _lsbLength;
	    private byte _checksum;
	    private byte _frameLength;
	    private bool _complete;
	    private byte _errorCode;
	}

	public class XBeeAddress
	{
	}

	/**
	 * Represents a 64-bit XBee Address
	 *
	 */
	public class XBeeAddress64 : XBeeAddress 
	{
		public XBeeAddress64(UInt64 addr)
		{
			_msb = (UInt32)(addr >> 32);
			_lsb = (UInt32)addr;
		}
		public XBeeAddress64(UInt32 msb, UInt32 lsb) 
		{
			_msb = msb;
			_lsb = lsb;
		}
		public XBeeAddress64()
		{
			_msb = 0;
			_lsb = 0;
		}
		public UInt32 getMsb() { return _msb; }
		public UInt32 getLsb() { return _lsb; }
		public UInt64 get() { return ((UInt64)(_msb) << 32) | _lsb; }
		public static implicit operator UInt64(XBeeAddress64 addr)
		{ 
			return addr.get(); 
		}
		public void setMsb(UInt32 msb) { _msb = msb; }
		public void setLsb(UInt32 lsb) { _lsb = lsb; }
		public void set(UInt64 addr)
		{
			_msb = (UInt32)(addr >> 32);
			_lsb = (UInt32)addr;
		}

		private UInt32 _msb;
		private UInt32 _lsb;
	}



	/**
	 * This class is extended by all Responses that include a frame id
	 */
	public class FrameIdResponse : XBeeResponse 
	{
		public FrameIdResponse()
		{
		}

		public byte getFrameId()
		{
			return getFrameData()[0];
		}

		private byte _frameId;
	}

	/**
	 * Common functionality for both Series 1 and 2 data RX data packets
	 */
	public abstract class RxDataResponse : XBeeResponse 
	{
		public RxDataResponse() : base() { }
		/**
		 * Returns the specified index of the payload.  The index may be 0 to getDataLength() - 1
		 * This method is deprecated; use uint8_t* getData()
		 */
		public byte getData(int index)
		{
			return getFrameData()[getDataOffset() + index];
		}

		/**
		 * Returns the payload array.  This may be accessed from index 0 to getDataLength() - 1
		 */
		public byte[] getData()
		{
			return getFrameData().Slice<byte>( getDataOffset(), getFrameDataLength());
		}

		/**
		 * Returns the length of the payload
		 */
		public abstract byte getDataLength();
		/**
		 * Returns the position in the frame data where the data begins
		 */
		public abstract byte getDataOffset();
	}

	// getResponse to return the proper subclass:
	// we maintain a pointer to each type of response, when a response is parsed, it is allocated only if NULL
	// can we allocate an object in a function?

#if SERIES_2
	/**
	 * Represents a Series 2 TX status packet
	 */
	public class ZBTxStatusResponse : FrameIdResponse 
	{
		public ZBTxStatusResponse() : base() { }
		public UInt16 getRemoteAddress()
		{
			return (byte)((getFrameData()[1] << 8) + getFrameData()[2]);
		}

		public byte getTxRetryCount()
		{
			return getFrameData()[3];
		}

		public byte getDeliveryStatus()
		{
			return getFrameData()[4];
		}

		public byte getDiscoveryStatus()
		{
			return getFrameData()[5];
		}

		public bool isSuccess()
		{
			return getDeliveryStatus() == XBee.SUCCESS;
		}

		public const byte API_ID = XBee.ZB_TX_STATUS_RESPONSE;
	}

	/**
	 * Represents a Series 2 RX packet
	 */
	public class ZBRxResponse : RxDataResponse 
	{
		public ZBRxResponse() : base()
		{
			_remoteAddress64 = new XBeeAddress64();
		}
		public XBeeAddress64 getRemoteAddress64() 
		{
			return _remoteAddress64;
		}

		public UInt16 getRemoteAddress16()
		{
			return (UInt16)((getFrameData()[8] << 8) + getFrameData()[9]);
		}

		public byte getOption()
		{
			return getFrameData()[10];
		}

		// markers to read data from packet array.  this is the index, so the 12th item in the array
		public override byte getDataOffset()
		{
			return 11;
		}

		public override byte getDataLength()
		{
			return (byte)(getPacketLength() - getDataOffset() - 1);
		}

		public const byte API_ID = XBee.ZB_RX_RESPONSE;
		private XBeeAddress64 _remoteAddress64;
	};

	/**
	 * Represents a Series 2 Explicit RX packet
	 *
	 * Note: The receive these responses, set AO=1. With the default AO=0,
	 * you will receive ZBRxResponses, not knowing exact details.
	 */
	public class ZBExplicitRxResponse : ZBRxResponse 
	{
		public ZBExplicitRxResponse() : base() { }
		public byte getSrcEndpoint()
		{
			return getFrameData()[10];
		}

		public byte getDstEndpoint()
		{
			return getFrameData()[11];
		}

		UInt16 getClusterId()
		{
			return (UInt16)((getFrameData()[12]) << 8 | getFrameData()[13]);
		}

		UInt16 getProfileId()
		{
			return (UInt16)((getFrameData()[14]) << 8 | getFrameData()[15]);
		}

		public new byte getOption()
		{
			return getFrameData()[16];
		}

		// markers to read data from packet array.
		public override byte getDataOffset()
		{
			return 17;
		}

		public override byte getDataLength()
		{
			return (byte)(getPacketLength() - getDataOffset() - 1);
		}

		public new const byte API_ID = XBee.ZB_EXPLICIT_RX_RESPONSE;
	};

	/**
	 * Represents a Series 2 RX I/O Sample packet
	 */
	public class ZBRxIoSampleResponse : ZBRxResponse 
	{
		public ZBRxIoSampleResponse() : base() { }
		public bool containsAnalog()
		{
			return getAnalogMask() > 0;
		}

		public bool containsDigital()
		{
			return getDigitalMaskMsb() > 0 || getDigitalMaskLsb() > 0;
		}
		/**
		 * Returns true if the pin is enabled
		 */
		public bool isAnalogEnabled(byte pin)
		{
			return ((getAnalogMask() >> pin) & 1) == 1;
		}
		/**
		 * Returns true if the pin is enabled
		 */
		public bool isDigitalEnabled(byte pin)
		{
			if (pin <= 7)
			{
				// added extra parens to calm avr compiler
				return ((getDigitalMaskLsb() >> pin) & 1) == 1;
			}
			else
			{
				return ((getDigitalMaskMsb() >> (pin - 8)) & 1) == 1;
			}
		}
		/**
		 * Returns the 10-bit analog reading of the specified pin.
		 * Valid pins include ADC:xxx.
		 */
		public UInt16 getAnalog(byte pin)
		{
			// analog starts 13 bytes after sample size, if no dio enabled
			byte start = 15;

			if (containsDigital())
			{
				// make room for digital i/o
				start += 2;
			}

			// start depends on how many pins before this pin are enabled
			for (int i = 0; i < pin; i++)
			{
				if (isAnalogEnabled((byte)i))
				{
					start += 2;
				}
			}

			return (UInt16)((getFrameData()[start] << 8) + getFrameData()[start + 1]);
		}

		/**
		 * Returns true if the specified pin is high/on.
		 * Valid pins include DIO:xxx.
		 */
		public bool isDigitalOn(byte pin)
		{
			if (pin <= 7)
			{
				// D0-7
				// DIO LSB is index 5
				return ((getFrameData()[16] >> pin) & 1) == 1;
			}
			else
			{
				// D10-12
				// DIO MSB is index 4
				return ((getFrameData()[15] >> (pin - 8)) & 1) == 1;
			}
		}

		// 64 + 16 addresses, sample size, option = 12 (index 11), so this starts at 12
		public byte getDigitalMaskMsb()
		{
			return (byte)(getFrameData()[12] & 0x1c);
		}

		public byte getDigitalMaskLsb()
		{
			return getFrameData()[13];
		}
		public byte getAnalogMask()
		{
			return (byte)(getFrameData()[14] & 0x8f);
		}

		public new const byte API_ID = XBee.ZB_IO_SAMPLE_RESPONSE;
	};

#endif

#if SERIES_1
	/**
	 * Represents a Series 1 TX Status packet
	 */
	public class TxStatusResponse : FrameIdResponse 
	{
		public TxStatusResponse() : base() { }
		public byte getStatus()
		{
			return getFrameData()[1];
		}

		public bool isSuccess()
		{
			return getStatus() == XBee.SUCCESS;
		}

		public const byte API_ID = XBee.TX_STATUS_RESPONSE;
	};

	/**
	 * Represents a Series 1 RX packet
	 */
	public abstract class RxResponse : RxDataResponse 
	{
		public RxResponse() : base() { }
		// remember rssi is negative but this is unsigned byte so it's up to you to convert
		public byte getRssi()
		{
			return getFrameData()[getRssiOffset()];
		}

		public byte getOption()
		{
			return getFrameData()[getRssiOffset() + 1];
		}

		bool isAddressBroadcast()
		{
			return (getOption() & 2) == 2;
		}

		bool isPanBroadcast()
		{
			return (getOption() & 4) == 4;
		}

		public override byte getDataLength()
		{
			return (byte)(getPacketLength() - getDataOffset() - 1);
		}

		public override byte getDataOffset()
		{
			return (byte)(getRssiOffset() + 2);
		}

		public abstract byte getRssiOffset();
	};

	/**
	 * Represents a Series 1 16-bit address RX packet
	 */
	public class Rx16Response : RxResponse 
	{
		public Rx16Response() : base() 
		{
			_remoteAddress = 0;
		}
		public override byte getRssiOffset()
		{
			return XBee.RX_16_RSSI_OFFSET;
		}

		public UInt16 getRemoteAddress16()
		{
			return (UInt16)((getFrameData()[0] << 8) + getFrameData()[1]);
		}

		public const byte API_ID = XBee.RX_16_RESPONSE;
		protected UInt16 _remoteAddress;
	};

	/**
	 * Represents a Series 1 64-bit address RX packet
	 */
	public class Rx64Response : RxResponse 
	{
		public Rx64Response() : base()
		{
			_remoteAddress = new XBeeAddress64();
		}

		public override byte getRssiOffset()
		{
			return XBee.RX_64_RSSI_OFFSET;
		}

		public XBeeAddress64 getRemoteAddress64() 
		{
			return _remoteAddress;
		}


		public const byte API_ID = XBee.RX_64_RESPONSE;
		private XBeeAddress64 _remoteAddress;
	};

	/**
	 * Represents a Series 1 RX I/O Sample packet
	 */
	public abstract class RxIoSampleBaseResponse : RxResponse 
	{
		public RxIoSampleBaseResponse() : base() { }

		/**
		 * Returns the number of samples in this packet
		 */
		public byte getSampleOffset()
		{
			// sample starts 2 bytes after rssi
			return (byte)(getRssiOffset() + 2);
		}

		public bool containsAnalog()
		{
			return (getFrameData()[getSampleOffset() + 1] & 0x7e) > 0;
		}

		public bool containsDigital()
		{
			return (getFrameData()[getSampleOffset() + 1] & 0x1) > 0 || getFrameData()[getSampleOffset() + 2] > 0;
		}

		/**
		 * Returns true if the specified analog pin is enabled
		 */
		public bool isAnalogEnabled(byte pin)
		{
			return (((getFrameData()[getSampleOffset() + 1] >> (pin + 1)) & 1) == 1);
		}
		/**
		 * Returns true if the specified digital pin is enabled
		 */
		public bool isDigitalEnabled(byte pin)
		{
			if (pin < 8)
			{
				return ((getFrameData()[getSampleOffset() + 2] >> pin) & 1) == 1;
			}
			else
			{
				return (getFrameData()[getSampleOffset() + 1] & 1) == 1;
			}
		}

		/**
		 * Returns the 10-bit analog reading of the specified pin.
		 * Valid pins include ADC:0-5.  Sample index starts at 0
		 */
		public UInt16 getAnalog(byte pin, byte sample)
		{
			byte start = getSampleStart(sample);

			if (containsDigital())
			{
				// Skip digital sample info
				start += 2;
			}

			// Skip any analog samples before this pin
			for (int i = 0; i < pin; i++)
			{
				if (isAnalogEnabled((byte)i))
				{
					start += 2;
				}
			}

			return (UInt16)((getFrameData()[start] << 8) + getFrameData()[start + 1]);
		}
		/**
		 * Returns true if the specified pin is high/on.
		 * Valid pins include DIO:0-8.  Sample index starts at 0
		 */
		public bool isDigitalOn(byte pin, byte sample)
		{
			if (pin < 8)
			{
				return ((getFrameData()[getSampleStart(sample) + 1] >> pin) & 1) == 1;
			}
			else
			{
				return (getFrameData()[getSampleStart(sample)] & 1) == 1;
			}
		}

		public byte getSampleSize()
		{
			return getFrameData()[getSampleOffset()];
		}

		/**
		 * Gets the offset of the start of the given sample.
		 */
		public byte getSampleStart(byte sample)
		{
			byte spacing = 0;

			if (containsDigital())
			{
				// make room for digital i/o sample (2 bytes per sample)
				spacing += 2;
			}

			// spacing between samples depends on how many are enabled. add
			// 2 bytes for each analog that's enabled
			for (int i = 0; i <= 5; i++)
			{
				if (isAnalogEnabled((byte)i))
				{
					// each analog is two bytes
					spacing += 2;
				}
			}

			// Skip 3-byte header and "sample" full samples
			return (byte)(getSampleOffset() + 3 + sample * spacing);
		}
	}

	public class Rx16IoSampleResponse : RxIoSampleBaseResponse 
	{
		public Rx16IoSampleResponse() : base() { }
		public UInt16 getRemoteAddress16()
		{
			return (UInt16)((getFrameData()[0] << 8) + getFrameData()[1]);
		}
		public override byte getRssiOffset()
		{
			return 2;
		}

		public const byte API_ID = XBee.RX_16_IO_RESPONSE;
	}

	public abstract class Rx64IoSampleResponse : RxIoSampleBaseResponse 
	{
		public Rx64IoSampleResponse() : base() 
		{
			_remoteAddress = new XBeeAddress64();
		}

		public XBeeAddress64 getRemoteAddress64() 
		{
			return _remoteAddress;
		}

		public override byte getRssiOffset()
		{
			return 8;
		}

		public const byte API_ID = XBee.RX_64_IO_RESPONSE;
		private XBeeAddress64 _remoteAddress;
	};

#endif

	/**
	 * Represents a Modem Status RX packet
	 */
	public class ModemStatusResponse : XBeeResponse 
	{
		public ModemStatusResponse() { }
		public byte getStatus()
		{
			return getFrameData()[0];
		}

		public const byte API_ID = XBee.MODEM_STATUS_RESPONSE;
	}

	/**
	 * Represents an AT Command RX packet
	 */
	public class AtCommandResponse : FrameIdResponse 
	{
		public AtCommandResponse() { }

		/**
		 * Returns an array containing the two character command
		 */
		public byte [] getCommand()
		{
			return new ArraySegment<byte>(getFrameData(),1,getFrameDataLength()-1).Array;
		}

		/**
		 * Returns the command status code.
		 * Zero represents a successful command
		 */
		public byte getStatus()
		{
			return getFrameData()[3];
		}

		/**
		 * Returns an array containing the command value.
		 * This is only applicable to query commands.
		 */
		public byte [] getValue()
		{
			if (getValueLength() > 0)
			{
				// value is only included for query commands.  set commands does not return a value
				return new ArraySegment<byte>(getFrameData(), 4, getFrameDataLength() - 4).Array;
			}

			return null;
		}

		/**
		 * Returns the length of the command value array.
		 */
		public byte getValueLength()
		{
			return (byte)(getFrameDataLength() - 4);
		}

		/**
		 * Returns true if status equals AT_OK
		 */
		public bool isOk()
		{
			return getStatus() == XBee.AT_OK;
		}

		public const byte API_ID = XBee.AT_COMMAND_RESPONSE;
	}

	/**
	 * Represents a Remote AT Command RX packet
	 */
	public class RemoteAtCommandResponse : AtCommandResponse 
	{
		public RemoteAtCommandResponse() : base() 
		{
			_remoteAddress64 = new XBeeAddress64();
		}
	/**
	 * Returns an array containing the two character command
	 */
	public new byte [] getCommand()
		{
			return new ArraySegment<byte>(getFrameData(),11,getFrameDataLength()-11).Array;
		}


		/**
		 * Returns the command status code.
		 * Zero represents a successful command
		 */
		public new byte getStatus()
		{
			return getFrameData()[13];
		}

		/**
		 * Returns an array containing the command value.
		 * This is only applicable to query commands.
		 */
		public new byte [] getValue()
		{
			if (getValueLength() > 0)
			{
				// value is only included for query commands.  set commands does not return a value
				return new ArraySegment<byte>(getFrameData(),14,getFrameDataLength()-14).Array;
			}

			return null;
		}

		/**
		 * Returns the length of the command value array.
		 */
		public new byte getValueLength()
		{
			return (byte)(getFrameDataLength() - 14);
		}

		/**
		 * Returns the 16-bit address of the remote radio
		 */
		public UInt16 getRemoteAddress16()
		{
			return (UInt16)((getFrameData()[9] << 8) + getFrameData()[10]);
		}
		/**
		 * Returns the 64-bit address of the remote radio
		 */

		public XBeeAddress64 getRemoteAddress64() 
		{
			return _remoteAddress64;
		}

		/**
		 * Returns true if command was successful
		 */
		public new bool isOk()
		{
			// weird c++ behavior.  w/o this method, it calls AtCommandResponse::isOk(), which calls the AtCommandResponse::getStatus, not this.getStatus!!!
			return getStatus() == XBee.AT_OK;
		}

		public new const byte API_ID = XBee.REMOTE_AT_COMMAND_RESPONSE;
		private XBeeAddress64 _remoteAddress64;
	}


	/**
	 * Super class of all XBee requests (TX packets)
	 * Users should never create an instance of this class; instead use an subclass of this class
	 * It is recommended to reuse Subclasses of the class to conserve memory
	 * <p/>
	 * This class allocates a buffer to
	 */
	public abstract class XBeeRequest
	{

		/**
		 * Constructor
		 * TODO make protected
		 */
		public XBeeRequest(byte apiId, byte frameId) 
		{
			_apiId = apiId;
			_frameId = frameId;
		}

		/**
		 * Sets the frame id.  Must be between 1 and 255 inclusive to get a TX status response.
		 */
		public void setFrameId(byte frameId)
		{
			_frameId = frameId;
		}

		/**
		 * Returns the frame id
		 */
		public byte getFrameId()
		{
			return _frameId;
		}

		/**
		 * Returns the API id
		 */
		public byte getApiId()
		{
			return _apiId;
		}

		// setting = 0 makes this a pure virtual function, meaning the subclass must implement, like abstract in java
		/**
		 * Starting after the frame id (pos = 0) and up to but not including the checksum
		 * Note: Unlike Digi's definition of the frame data, this does not start with the API ID.
		 * The reason for this is the API ID and Frame ID are common to all requests, whereas my definition of
		 * frame data is only the API specific data.
		 */
		public abstract byte getFrameData(byte pos);

		/**
		 * Returns the size of the api frame (not including frame id or api id or checksum).
		 */
		public abstract byte getFrameDataLength();

		protected void setApiId(byte apiId)
		{
			_apiId = apiId;
		}
		private byte _apiId;
		private byte _frameId;
	}

	// TODO add reset/clear method since responses are often reused
	/**
	 * Primary interface for communicating with an XBee Radio.
	 * This class provides methods for sending and receiving packets with an XBee radio via the serial port.
	 * The XBee radio must be configured in API (packet) mode (AP=2)
	 * in order to use this software.
	 * <p/>
	 * Since this code is designed to run on a microcontroller, with only one thread, you are responsible for reading the
	 * data off the serial buffer in a timely manner.  This involves a call to a variant of readPacket(...).
	 * If your serial port is receiving data faster than you are reading, you can expect to lose packets.
	 * Arduino only has a 128 byte serial buffer so it can easily overflow if two or more packets arrive
	 * without a call to readPacket(...)
	 * <p/>
	 * In order to conserve resources, this class only supports storing one response packet in memory at a time.
	 * This means that you must fully consume the packet prior to calling readPacket(...), because calling
	 * readPacket(...) overwrites the previous response.
	 * <p/>
	 * This class creates an array of size MAX_FRAME_DATA_SIZE for storing the response packet.  You may want
	 * to adjust this value to conserve memory.
	 *
	 * \author Andrew Rapp
	 */
	public class XBee
	{
		// set to ATAP value of XBee. AP=2 is recommended
		public const int ATAP = 2;

		public const int START_BYTE = 0x7e;
		public const int ESCAPE = 0x7d;
		public const int XON = 0x11;
		public const int XOFF = 0x13;

		// This value determines the size of the byte array for receiving RX packets
		// Most users won't be dealing with packets this large so you can adjust this
		// value to reduce memory consumption. But, remember that
		// if a RX packet exceeds this size, it cannot be parsed!

		// This value is determined by the largest packet size (100 byte payload + 64-bit address + option byte and rssi byte) of a series 1 radio
		public const int MAX_FRAME_DATA_SIZE = 110;

		public const int BROADCAST_ADDRESS = 0xffff;
		public const int ZB_BROADCAST_ADDRESS = 0xfffe;

		// the non-variable length of the frame data (not including frame id or api id or variable data size (e.g. payload, at command set value)
		public const int ZB_TX_API_LENGTH = 12;
		public const int ZB_EXPLICIT_TX_API_LENGTH = 18;
		public const int TX_16_API_LENGTH = 3;
		public const int TX_64_API_LENGTH = 9;
		public const int AT_COMMAND_API_LENGTH = 2;
		public const int REMOTE_AT_COMMAND_API_LENGTH = 13;
		// start/length(2)/api/frameid/checksum bytes
		public const int PACKET_OVERHEAD_LENGTH = 6;
		// api is always the third byte in packet
		public const int API_ID_INDEX = 3;

		// frame position of rssi byte
		public const int RX_16_RSSI_OFFSET = 2;
		public const int RX_64_RSSI_OFFSET = 8;

		public const int DEFAULT_FRAME_ID = 1;
		public const int NO_RESPONSE_FRAME_ID = 0;

		// These are the parameters used by the XBee ZB modules when you do a
		// regular "ZB TX request".
		public const int DEFAULT_ENDPOINT = 232;
		public const int DEFAULT_CLUSTER_ID = 0x0011;
		public const int DEFAULT_PROFILE_ID = 0xc105;

		// TODO put in tx16 class
		public const int ACK_OPTION = 0;
		public const int DISABLE_ACK_OPTION = 1;
		public const int BROADCAST_OPTION = 4;

		// RX options
		public const int ZB_PACKET_ACKNOWLEDGED = 0x01;
		public const int ZB_BROADCAST_PACKET = 0x02;

		// not everything is implemented!
		/**
		 * Api Id constants
		 */
		public const int TX_64_REQUEST = 0x0;
		public const int TX_16_REQUEST = 0x1;
		public const int AT_COMMAND_REQUEST = 0x08;
		public const int AT_COMMAND_QUEUE_REQUEST = 0x09;
		public const int REMOTE_AT_REQUEST = 0x17;
		public const int ZB_TX_REQUEST = 0x10;
		public const int ZB_EXPLICIT_TX_REQUEST = 0x11;
		public const int RX_64_RESPONSE = 0x80;
		public const int RX_16_RESPONSE = 0x81;
		public const int RX_64_IO_RESPONSE = 0x82;
		public const int RX_16_IO_RESPONSE = 0x83;
		public const int AT_RESPONSE = 0x88;
		public const int TX_STATUS_RESPONSE = 0x89;
		public const int MODEM_STATUS_RESPONSE = 0x8a;
		public const int ZB_RX_RESPONSE = 0x90;
		public const int ZB_EXPLICIT_RX_RESPONSE = 0x91;
		public const int ZB_TX_STATUS_RESPONSE = 0x8b;
		public const int ZB_IO_SAMPLE_RESPONSE = 0x92;
		public const int ZB_IO_NODE_IDENTIFIER_RESPONSE = 0x95;
		public const int AT_COMMAND_RESPONSE = 0x88;
		public const int REMOTE_AT_COMMAND_RESPONSE = 0x97;


		/**
		 * TX STATUS constants
		 */
		public const int SUCCESS = 0x0;
		public const int CCA_FAILURE = 0x2;
		public const int INVALID_DESTINATION_ENDPOINT_SUCCESS = 0x15;
		public const int NETWORK_ACK_FAILURE = 0x21;
		public const int NOT_JOINED_TO_NETWORK = 0x22;
		public const int SELF_ADDRESSED = 0x23;
		public const int ADDRESS_NOT_FOUND = 0x24;
		public const int ROUTE_NOT_FOUND = 0x25;
		public const int PAYLOAD_TOO_LARGE = 0x74;
		// Returned by XBeeWithCallbacks::waitForStatus on timeout
		public const int XBEE_WAIT_TIMEOUT = 0xff;

		// modem status
		public const int HARDWARE_RESET = 0;
		public const int WATCHDOG_TIMER_RESET = 1;
		public const int ASSOCIATED = 2;
		public const int DISASSOCIATED = 3;
		public const int SYNCHRONIZATION_LOST = 4;
		public const int COORDINATOR_REALIGNMENT = 5;
		public const int COORDINATOR_STARTED = 6;

		public const int ZB_BROADCAST_RADIUS_MAX_HOPS = 0;

		public const int ZB_TX_UNICAST = 0;
		public const int ZB_TX_BROADCAST = 8;

		public const int AT_OK = 0;
		public const int AT_ERROR = 1;
		public const int AT_INVALID_COMMAND = 2;
		public const int AT_INVALID_PARAMETER = 3;
		public const int AT_NO_RESPONSE = 4;

		public const int NO_ERROR = 0;
		public const int CHECKSUM_FAILURE = 1;
		public const int PACKET_EXCEEDS_BYTE_ARRAY_LENGTH = 2;
		public const int UNEXPECTED_START_BYTE = 3;



		public XBee()
		{
			_pos = 0;
			_escape = false;
			_checksumTotal = 0;
			_nextFrameId = 0;

			_response = new XBeeResponse();
			_response.init();
			_response.setFrameData(_responseFrameData);
			// Contributed by Paul Stoffregen for Teensy support
			_serial = null;
		}

		/**
		 * Reads all available serial bytes until a packet is parsed, an error occurs, or the buffer is empty.
		 * You may call <i>xbee</i>.getResponse().isAvailable() after calling this method to determine if
		 * a packet is ready, or <i>xbee</i>.getResponse().isError() to determine if
		 * a error occurred.
		 * <p/>
		 * This method should always return quickly since it does not wait for serial data to arrive.
		 * You will want to use this method if you are doing other timely stuff in your loop, where
		 * a delay would cause problems.
		 * NOTE: calling this method resets the current response, so make sure you first consume the
		 * current response
		 */
		public void readPacket()
		{
			// reset previous response
			if (_response.isAvailable() || _response.isError())
			{
				// discard previous packet and start over
				resetResponse();
			}

			while (available())
			{

				b = read();

				if (_pos > 0 && b == START_BYTE && ATAP == 2)
				{
					// new packet start before previous packeted completed -- discard previous packet and start over
					_response.setErrorCode(UNEXPECTED_START_BYTE);
					return;
				}

				if (_pos > 0 && b == ESCAPE)
				{
					if (available())
					{
						b = read();
						b = (byte)(0x20 ^ b);
					}
					else
					{
						// escape byte.  next byte will be
						_escape = true;
						continue;
					}
				}

				if (_escape == true)
				{
					b = (byte)(0x20 ^ b);
					_escape = false;
				}

				// checksum includes all bytes starting with api id
				if (_pos >= API_ID_INDEX)
				{
					_checksumTotal += b;
				}

				switch (_pos)
				{
					case 0:
						if (b == START_BYTE)
						{
							_pos++;
						}

						break;
					case 1:
						// length msb
						_response.setMsbLength(b);
						_pos++;

						break;
					case 2:
						// length lsb
						_response.setLsbLength(b);
						_pos++;

						break;
					case 3:
						_response.setApiId(b);
						_pos++;

						break;
					default:
						// starts at fifth byte

						if (_pos > MAX_FRAME_DATA_SIZE)
						{
							// exceed max size.  should never occur
							_response.setErrorCode(PACKET_EXCEEDS_BYTE_ARRAY_LENGTH);
							return;
						}

						// check if we're at the end of the packet
						// packet length does not include start, length, or checksum bytes, so add 3
						if (_pos == (_response.getPacketLength() + 3))
						{
							// verify checksum

							if ((_checksumTotal & 0xff) == 0xff)
							{
								_response.setChecksum(b);
								_response.setAvailable(true);

								_response.setErrorCode(NO_ERROR);
							}
							else
							{
								// checksum failed
								_response.setErrorCode(CHECKSUM_FAILURE);
							}

							// minus 4 because we start after start,msb,lsb,api and up to but not including checksum
							// e.g. if frame was one byte, _pos=4 would be the byte, pos=5 is the checksum, where end stop reading
							_response.setFrameLength((byte)(_pos - 4));

							// reset state vars
							_pos = 0;

							return;
						}
						else
						{
							// add to packet array, starting with the fourth byte of the apiFrame
							_response.getFrameData()[_pos - 4] = b;
							_pos++;
						}
						break;
				}
			}
		}

		/**
		 * Waits a maximum of <i>timeout</i> milliseconds for a response packet before timing out; returns true if packet is read.
		 * Returns false if timeout or error occurs.
		 */
		public bool readPacket(int timeout)
		{

			if (timeout < 0)
			{
				return false;
			}

			Stopwatch timer = Stopwatch.StartNew();

			while (timer.ElapsedMilliseconds < timeout)
			{

				readPacket();

				if (getResponse().isAvailable())
				{
					return true;
				}
				else if (getResponse().isError())
				{
					return false;
				}
			}

			// timed out
			return false;
		}


		/**
		 * Reads until a packet is received or an error occurs.
		 * Caution: use this carefully since if you don't get a response, your Arduino code will hang on this
		 * call forever!! often it's better to use a timeout: readPacket(int)
		 */
		public void readPacketUntilAvailable()
		{
			while (!(getResponse().isAvailable() || getResponse().isError()))
			{
				// read some more
				readPacket();
			}
		}

		/**
		 * Starts the serial connection on the specified serial port
		 */
		// Support for SoftwareSerial. Contributed by Paul Stoffregen
		public void begin(SerialPort serial)
		{
			_serial = serial;
		}

		/**
		 * Returns a reference to the current response
		 * Note: once readPacket is called again this response will be overwritten!
		 */
		public XBeeResponse getResponse()
		{
			return _response;
		}

		// TODO how to convert response to proper subclass?
		public void getResponse(XBeeResponse response)
		{

			response.setMsbLength(_response.getMsbLength());
			response.setLsbLength(_response.getLsbLength());
			response.setApiId(_response.getApiId());
			response.setFrameLength(_response.getFrameDataLength());

			response.setFrameData(_response.getFrameData());
		}


		/**
		 * Sends a XBeeRequest (TX packet) out the serial port
		 */
		public void send(XBeeRequest request)
		{
			// the new new deal
			byte[] buf = new byte[request.getFrameDataLength() + 2 + 4];
			int index = 0;

			buf[index++] = XBee.START_BYTE;

			// send length
			byte msbLen = (byte)(((request.getFrameDataLength() + 2) >> 8) & 0xff);
			byte lsbLen = (byte)((request.getFrameDataLength() + 2) & 0xff);

			buf[index++] = msbLen;
			buf[index++] = lsbLen;

			// api id
			buf[index++] = request.getApiId();
			buf[index++] = request.getFrameId();

			byte checksum = 0;

			// compute checksum, start at api id
			checksum += request.getApiId();
			checksum += request.getFrameId();

			for (int i = 0; i < request.getFrameDataLength(); i++)
			{
				buf[index++] = request.getFrameData((byte)i);
				checksum += request.getFrameData((byte)i);
			}

			// perform 2s complement
			checksum = (byte)(0xff - checksum);

			// send checksum
			buf[index++] = checksum;

			write(buf, buf.Length);
			//for (int i = 0; i < buf.Length; i++)
			//{
			//	Console.Write(buf[i].ToString("X2") + " ");
			//}
			//Console.WriteLine();
		}

		/**
		 * Returns a sequential frame id between 1 and 255
		 */
		public byte getNextFrameId()
		{
			_nextFrameId++;

			if (_nextFrameId == 0)
			{
				// can't send 0 because that disables status response
				_nextFrameId = 1;
			}

			return _nextFrameId;
		}

		/**
		 * Specify the serial port.  Only relevant for Arduinos that support multiple serial ports (e.g. Mega)
		 */
		public void setSerial(SerialPort serial)
		{
			_serial = serial;
		}

		private bool available()
		{
			return _serial.BytesToRead > 0 ;
		}

		private byte read()
		{
			return (byte)_serial.ReadByte();
		}
		//void flush();
		private void write(byte val)
		{
			byte[] buf = new byte[] { val };
			_serial.Write(buf, 0, 1);
		}
		private void write(byte [] buf, int len)
		{
			_serial.Write(buf, 0, len);
		}
		private void sendByte(byte b, bool escape)
		{

			if (escape && (b == START_BYTE || b == ESCAPE || b == XON || b == XOFF))
			{
				write(ESCAPE);
				write((byte)(b ^ 0x20));
			}
			else
			{
				write(b);
			}
		}

		private void resetResponse()
		{
			_pos = 0;
			_escape = false;
			_checksumTotal = 0;
			_response.reset();
		}


		private XBeeResponse _response;
		private bool _escape;
		// current packet position for response.  just a state variable for packet parsing and has no relevance for the response otherwise
		private byte _pos;
		// last byte read
		private byte b;
		private byte _checksumTotal;
		private byte _nextFrameId;
		// buffer for incoming RX packets.  holds only the api specific frame data, starting after the api id byte and prior to checksum
		private byte [] _responseFrameData = new byte [XBee.MAX_FRAME_DATA_SIZE];
		private SerialPort _serial;
	}


///**
// * This class can be used instead of the XBee class and allows
// * user-specified callback functions to be called when responses are
// * received, simplifying the processing code and reducing boilerplate.
// *
// * To use it, first register your callback functions using the onXxx
// * methods. Each method has a uintptr_t data argument, that can be used to
// * pass arbitrary data to the callback (useful when using the same
// * function for multiple callbacks, or have a generic function that can
// * behave differently in different circumstances). Supplying the data
// * parameter is optional, but the callback must always accept it (just
// * ignore it if it's unused). The uintptr_t type is an integer type
// * guaranteed to be big enough to fit a pointer (it is 16-bit on AVR,
// * 32-bit on ARM), so it can also be used to store a pointer to access
// * more data if required (using proper casts).
// *
// * There can be only one callback of each type registered at one time,
// * so registering callback overwrites any previously registered one. To
// * unregister a callback, pass NULL as the function.
// *
// * To ensure that the callbacks are actually called, call the loop()
// * method regularly (in your loop() function, for example). This takes
// * care of calling readPacket() and getResponse() other methods on the
// * XBee class, so there is no need to do so directly (though it should
// * not mess with this class if you do, it would only mean some callbacks
// * aren't called).
// *
// * Inside callbacks, you should generally not be blocking / waiting.
// * Since callbacks can be called from inside waitFor() and friends, a
// * callback that doesn't return quickly can mess up the waitFor()
// * timeout.
// *
// * Sending packets is not a problem inside a callback, but avoid
// * receiving a packet (e.g. calling readPacket(), loop() or waitFor()
// * and friends) inside a callback (since that would overwrite the
// * current response, messing up any pending callbacks and waitFor() etc.
// * methods already running).
// */
//class XBeeWithCallbacks : public XBee {
//public:

//	/**
//	 * Register a packet error callback. It is called whenever an
//	 * error occurs in the packet reading process. Arguments to the
//	 * callback will be the error code (as returned by
//	 * XBeeResponse::getErrorCode()) and the data parameter.  while
//	 * registering the callback.
//	 */
//	void onPacketError(void (* func)(uint8_t, uintptr_t), uintptr_t data = 0) { _onPacketError.set(func, data); }

///**
// * Register a response received callback. It is called whenever
// * a response was succesfully received, before a response
// * specific callback (or onOtherResponse) below is called.
// *
// * Arguments to the callback will be the received response and
// * the data parameter passed while registering the callback.
// */
//void onResponse(void (* func)(XBeeResponse&, uintptr_t), uintptr_t data = 0) { _onResponse.set(func, data); }

//	/**
//	 * Register an other response received callback. It is called
//	 * whenever a response was succesfully received, but no response
//	 * specific callback was registered using the functions below
//	 * (after the onResponse callback is called).
//	 *
//	 * Arguments to the callback will be the received response and
//	 * the data parameter passed while registering the callback.
//	 */
//	void onOtherResponse(void (* func)(XBeeResponse&, uintptr_t), uintptr_t data = 0) { _onOtherResponse.set(func, data); }

//	// These functions register a response specific callback. They
//	// are called whenever a response of the appropriate type was
//	// succesfully received (after the onResponse callback is
//	// called).
//	//
//	// Arguments to the callback will be the received response
//	// (already converted to the appropriate type) and the data
//	// parameter passed while registering the callback.
//	void onZBTxStatusResponse(void (* func)(ZBTxStatusResponse&, uintptr_t), uintptr_t data = 0) { _onZBTxStatusResponse.set(func, data); }
//	void onZBRxResponse(void (* func)(ZBRxResponse&, uintptr_t), uintptr_t data = 0) { _onZBRxResponse.set(func, data); }
//	void onZBExplicitRxResponse(void (* func)(ZBExplicitRxResponse&, uintptr_t), uintptr_t data = 0) { _onZBExplicitRxResponse.set(func, data); }
//	void onZBRxIoSampleResponse(void (* func)(ZBRxIoSampleResponse&, uintptr_t), uintptr_t data = 0) { _onZBRxIoSampleResponse.set(func, data); }
//	void onTxStatusResponse(void (* func)(TxStatusResponse&, uintptr_t), uintptr_t data = 0) { _onTxStatusResponse.set(func, data); }
//	void onRx16Response(void (* func)(Rx16Response&, uintptr_t), uintptr_t data = 0) { _onRx16Response.set(func, data); }
//	void onRx64Response(void (* func)(Rx64Response&, uintptr_t), uintptr_t data = 0) { _onRx64Response.set(func, data); }
//	void onRx16IoSampleResponse(void (* func)(Rx16IoSampleResponse&, uintptr_t), uintptr_t data = 0) { _onRx16IoSampleResponse.set(func, data); }
//	void onRx64IoSampleResponse(void (* func)(Rx64IoSampleResponse&, uintptr_t), uintptr_t data = 0) { _onRx64IoSampleResponse.set(func, data); }
//	void onModemStatusResponse(void (* func)(ModemStatusResponse&, uintptr_t), uintptr_t data = 0) { _onModemStatusResponse.set(func, data); }
//	void onAtCommandResponse(void (* func)(AtCommandResponse&, uintptr_t), uintptr_t data = 0) { _onAtCommandResponse.set(func, data); }
//	void onRemoteAtCommandResponse(void (* func)(RemoteAtCommandResponse&, uintptr_t), uintptr_t data = 0) { _onRemoteAtCommandResponse.set(func, data); }

//	/**
//	 * Regularly call this method, which ensures that the serial
//	 * buffer is processed and the appropriate callbacks are called.
//	 */
//	void loop();

///**
// * Wait for a API response of the given type, optionally
// * filtered by the given match function.
// *
// * If a match function is given it is called for every response
// * of the right type received, passing the response and the data
// * parameter passed to this method. If the function returns true
// * (or if no function was passed), waiting stops and this method
// * returns 0. If the function returns false, waiting
// * continues. After the given timeout passes, this method
// * returns XBEE_WAIT_TIMEOUT.
// *
// * If a valid frameId is passed (e.g. 0-255 inclusive) and a
// * status API response frame is received while waiting, that has
// * a *non-zero* status, waiting stops and that status is
// * received. This is intended for when a TX packet was sent and
// * you are waiting for an RX reply, which will most likely never
// * arrive when TX failed. However, since the status reply is not
// * guaranteed to arrive before the RX reply (a remote module can
// * send a reply before the ACK), first calling waitForStatus()
// * and then waitFor() can sometimes miss the reply RX packet.
// *
// * Note that when the intended response is received *before* the
// * status reply, the latter will not be processed by this
// * method and will be subsequently processed by e.g. loop()
// * normally.
// *
// * While waiting, any other responses received are passed to the
// * relevant callbacks, just as if calling loop() continuously
// * (except for the response sought, that one is only passed to
// * the OnResponse handler and no others).
// *
// * After this method returns, the response itself can still be
// * retrieved using getResponse() as normal.
// */
//template<typename Response>
//uint8_t waitFor(Response& response, uint16_t timeout, bool (* func)(Response&, uintptr_t) = NULL, uintptr_t data = 0, int16_t frameId = -1) {
//		return waitForInternal(Response::API_ID, &response, timeout, (void*) func, data, frameId);
//	}

//	/**
//	 * Sends a XBeeRequest (TX packet) out the serial port, and wait
//	 * for a status response API frame (up until the given timeout).
//	 * Essentially this just calls send() and waitForStatus().
//	 * See waitForStatus for the meaning of the return value and
//	 * more details.
//	 */
//	uint8_t sendAndWait(XBeeRequest &request, uint16_t timeout)
//{
//	send(request);
//	return waitForStatus(request.getFrameId(), timeout);
//}

///**
// * Wait for a status API response with the given frameId and
// * return the status from the packet (for ZB_TX_STATUS_RESPONSE,
// * this returns just the delivery status, not the routing
// * status). If the timeout is reached before reading the
// * response, XBEE_WAIT_TIMEOUT is returned instead.
// *
// * While waiting, any other responses received are passed to the
// * relevant callbacks, just as if calling loop() continuously
// * (except for the status response sought, that one is only
// * passed to the OnResponse handler and no others).
// *
// * After this method returns, the response itself can still be
// * retrieved using getResponse() as normal.
// */
//uint8_t waitForStatus(uint8_t frameId, uint16_t timeout);
//private:
//	/**
//	 * Internal version of waitFor that does not need to be
//	 * templated (to prevent duplication the implementation for
//	 * every response type you might want to wait for). Instead of
//	 * using templates, this accepts the apiId to wait for and will
//	 * cast the given response object and the argument to the given
//	 * function to the corresponding type. This means that the
//	 * void* given must match the api id!
//	 */
//	uint8_t waitForInternal(uint8_t apiId, void* response, uint16_t timeout, void* func, uintptr_t data, int16_t frameId);

///**
// * Helper that checks if the current response is a status
// * response with the given frame id. If so, returns the status
// * byte from the response, otherwise returns 0xff.
// */
//uint8_t matchStatus(uint8_t frameId);

///**
// * Top half of a typical loop(). Calls readPacket(), calls
// * onPacketError on error, calls onResponse when a response is
// * available. Returns in the true in the latter case, after
// * which a caller should typically call loopBottom().
// */
//bool loopTop();

///**
// * Bottom half of a typical loop. Call only when a valid
// * response was read, will call all response-specific callbacks.
// */
//void loopBottom();

//template<typename Arg> struct Callback
//{
//	void (* func) (Arg, uintptr_t);
//		uintptr_t data;
//	void set(void (* func)(Arg, uintptr_t), uintptr_t data)
//	{
//		this->func = func;
//		this->data = data;
//	}
//	bool call(Arg arg)
//	{
//		if (this->func)
//		{
//			this->func(arg, this->data);
//			return true;
//		}
//		return false;
//	}
//};

//Callback<uint8_t> _onPacketError;
//Callback<XBeeResponse&> _onResponse;
//Callback<XBeeResponse&> _onOtherResponse;
//Callback<ZBTxStatusResponse&> _onZBTxStatusResponse;
//Callback<ZBRxResponse&> _onZBRxResponse;
//Callback<ZBExplicitRxResponse&> _onZBExplicitRxResponse;
//Callback<ZBRxIoSampleResponse&> _onZBRxIoSampleResponse;
//Callback<TxStatusResponse&> _onTxStatusResponse;
//Callback<Rx16Response&> _onRx16Response;
//Callback<Rx64Response&> _onRx64Response;
//Callback<Rx16IoSampleResponse&> _onRx16IoSampleResponse;
//Callback<Rx64IoSampleResponse&> _onRx64IoSampleResponse;
//Callback<ModemStatusResponse&> _onModemStatusResponse;
//Callback<AtCommandResponse&> _onAtCommandResponse;
//Callback<RemoteAtCommandResponse&> _onRemoteAtCommandResponse;
//};

	/**
	 * All TX packets that support payloads extend this class
	 */
	public abstract class PayloadRequest : XBeeRequest 
	{
		public PayloadRequest(byte apiId, byte frameId, byte [] payload, byte payloadLength) : base(apiId, frameId)
		{
			_payloadPtr = payload;
			_payloadLength = payloadLength;
		}

		/**
		 * Returns the payload of the packet, if not null
		 */
		public byte [] getPayload()
		{
			return _payloadPtr;
		}

		/**
		 * Sets the payload array
		 */
		public void setPayload(byte [] payload)
		{
			_payloadPtr = payload;
		}


		/*
		 * Set the payload and its length in one call.
		 */
		public void setPayload(byte [] payloadPtr, byte payloadLength)
		{
			setPayload(payloadPtr);
			setPayloadLength(payloadLength);
		}

		/**
		 * Returns the length of the payload array, as specified by the user.
		 */
		public byte getPayloadLength()
		{
			return _payloadLength;
		}

		/**
		 * Sets the length of the payload to include in the request.  For example if the payload array
		 * is 50 bytes and you only want the first 10 to be included in the packet, set the length to 10.
		 * Length must be <= to the array length.
		 */
		public void setPayloadLength(byte payloadLength)
		{
			_payloadLength = payloadLength;
		}

		private byte [] _payloadPtr;
		private byte _payloadLength;
	}

#if SERIES_1

	/**
	 * Represents a Series 1 TX packet that corresponds to Api Id: TX_16_REQUEST
	 * <p/>
	 * Be careful not to send a data array larger than the max packet size of your radio.
	 * This class does not perform any validation of packet size and there will be no indication
	 * if the packet is too large, other than you will not get a TX Status response.
	 * The datasheet says 100 bytes is the maximum, although that could change in future firmware.
	 */
	public class Tx16Request : PayloadRequest 
	{
		public Tx16Request(UInt16 addr16, byte option, byte [] data, byte dataLength, byte frameId) : base(XBee.TX_16_REQUEST, frameId, data, dataLength)
		{
			_addr16 = addr16;
			_option = option;
		}

		/**
		 * Creates a Unicast Tx16Request with the ACK option and DEFAULT_FRAME_ID
		 */
		public Tx16Request(UInt16 addr16, byte [] data, byte dataLength) : base(XBee.TX_16_REQUEST, XBee.DEFAULT_FRAME_ID, data, dataLength)
		{
			_addr16 = addr16;
			_option = XBee.ACK_OPTION;
		}

		/**
		 * Creates a default instance of this class.  At a minimum you must specify
		 * a payload, payload length and a destination address before sending this request.
		 */
		public Tx16Request() : base(XBee.TX_16_REQUEST, XBee.DEFAULT_FRAME_ID, null, 0)
		{
			_option = XBee.ACK_OPTION;
		}

		public UInt16 getAddress16()
		{
			return _addr16;
		}

		public void setAddress16(UInt16 addr16)
		{
			_addr16 = addr16;
		}

		public byte getOption()
		{
			return _option;
		}

		public void setOption(byte option)
		{
			_option = option;
		}

		public override byte getFrameData(byte pos)
		{

			if (pos == 0)
			{
				return (byte)((_addr16 >> 8) & 0xff);
			}
			else if (pos == 1)
			{
				return (byte)(_addr16 & 0xff);
			}
			else if (pos == 2)
			{
				return _option;
			}
			else
			{
				return getPayload()[pos - XBee.TX_16_API_LENGTH];
			}
		}

		public override byte getFrameDataLength()
		{
			return (byte)(XBee.TX_16_API_LENGTH + getPayloadLength());
		}


		private UInt16 _addr16;
		private byte _option;
	};

	/**
	 * Represents a Series 1 TX packet that corresponds to Api Id: TX_64_REQUEST
	 *
	 * Be careful not to send a data array larger than the max packet size of your radio.
	 * This class does not perform any validation of packet size and there will be no indication
	 * if the packet is too large, other than you will not get a TX Status response.
	 * The datasheet says 100 bytes is the maximum, although that could change in future firmware.
	 */
	public class Tx64Request : PayloadRequest 
	{
		public Tx64Request(XBeeAddress64 addr64, byte option, byte [] data, byte dataLength, byte frameId) : base(XBee.TX_64_REQUEST, frameId, data, dataLength)
		{
			_addr64 = addr64;
			_option = option;
		}

		/**
		 * Creates a unicast Tx64Request with the ACK option and DEFAULT_FRAME_ID
		 */
		public Tx64Request(XBeeAddress64 addr64, byte [] data, byte dataLength) : base(XBee.TX_64_REQUEST, XBee.DEFAULT_FRAME_ID, data, dataLength)
		{
			_addr64 = addr64;
			_option = XBee.ACK_OPTION;
		}
		/**
		 * Creates a default instance of this class.  At a minimum you must specify
		 * a payload, payload length and a destination address before sending this request.
		 */
		public Tx64Request() : base(XBee.TX_64_REQUEST, XBee.DEFAULT_FRAME_ID, null, 0)
		{
			_option = XBee.ACK_OPTION;
		}

		public XBeeAddress64 getAddress64() 
		{
			return _addr64;
		}
		public void setAddress64(XBeeAddress64 addr64)
		{
			_addr64 = addr64;
		}

		// TODO move option to superclass
		public byte getOption()
		{
			return _option;
		}

		public void setOption(byte option)
		{
			_option = option;
		}

		public override byte getFrameData(byte pos)
		{

			if (pos == 0)
			{
				return (byte)((_addr64.getMsb() >> 24) & 0xff);
			}
			else if (pos == 1)
			{
				return (byte)((_addr64.getMsb() >> 16) & 0xff);
			}
			else if (pos == 2)
			{
				return (byte)((_addr64.getMsb() >> 8) & 0xff);
			}
			else if (pos == 3)
			{
				return (byte)(_addr64.getMsb() & 0xff);
			}
			else if (pos == 4)
			{
				return (byte)((_addr64.getLsb() >> 24) & 0xff);
			}
			else if (pos == 5)
			{
				return (byte)((_addr64.getLsb() >> 16) & 0xff);
			}
			else if (pos == 6)
			{
				return (byte)((_addr64.getLsb() >> 8) & 0xff);
			}
			else if (pos == 7)
			{
				return (byte)(_addr64.getLsb() & 0xff);
			}
			else if (pos == 8)
			{
				return _option;
			}
			else
			{
				return getPayload()[pos - XBee.TX_64_API_LENGTH];
			}
		}
		public override byte getFrameDataLength()
		{
			return (byte)(XBee.TX_64_API_LENGTH + getPayloadLength());
		}

		private XBeeAddress64 _addr64;
		private byte _option;
	}

#endif


#if SERIES_2

	/**
	 * Represents a Series 2 TX packet that corresponds to Api Id: ZB_TX_REQUEST
	 *
	 * Be careful not to send a data array larger than the max packet size of your radio.
	 * This class does not perform any validation of packet size and there will be no indication
	 * if the packet is too large, other than you will not get a TX Status response.
	 * The datasheet says 72 bytes is the maximum for ZNet firmware and ZB Pro firmware provides
	 * the ATNP command to get the max supported payload size.  This command is useful since the
	 * maximum payload size varies according to certain settings, such as encryption.
	 * ZB Pro firmware provides a PAYLOAD_TOO_LARGE that is returned if payload size
	 * exceeds the maximum.
	 */
	public class ZBTxRequest : PayloadRequest 
	{
		/**
		 * Creates a unicast ZBTxRequest with the ACK option and DEFAULT_FRAME_ID
		 */
		public ZBTxRequest( XBeeAddress64 addr64, UInt16 addr16, byte broadcastRadius, byte option, byte [] data, byte dataLength, byte frameId) : base(XBee.ZB_TX_REQUEST, frameId, data, dataLength)
		{
			_addr64 = addr64;
			_addr16 = addr16;
			_broadcastRadius = broadcastRadius;
			_option = option;
		}

		public ZBTxRequest(XBeeAddress64 addr64, byte [] data, byte dataLength) : base(XBee.ZB_TX_REQUEST, XBee.DEFAULT_FRAME_ID, data, dataLength)
		{
			_addr64 = addr64;
			_addr16 = XBee.ZB_BROADCAST_ADDRESS;
			_broadcastRadius = XBee.ZB_BROADCAST_RADIUS_MAX_HOPS;
			_option = XBee.ZB_TX_UNICAST;
		}
		/**
		 * Creates a default instance of this class.  At a minimum you must specify
		 * a payload, payload length and a 64-bit destination address before sending
		 * this request.
		 */
		public ZBTxRequest() : base(XBee.ZB_TX_REQUEST, XBee.DEFAULT_FRAME_ID, null, 0)
		{
			_addr16 = XBee.ZB_BROADCAST_ADDRESS;
			_broadcastRadius = XBee.ZB_BROADCAST_RADIUS_MAX_HOPS;
			_option = XBee.ZB_TX_UNICAST;
		}

		public XBeeAddress64 getAddress64() 
		{
			return _addr64;
		}

		public UInt16 getAddress16()
		{
			return _addr16;
		}

		public byte getBroadcastRadius()
		{
			return _broadcastRadius;
		}

		public byte getOption()
		{
			return _option;
		}

		public void setAddress64(XBeeAddress64 addr64)
		{
			_addr64 = addr64;
		}

		public void setAddress16(UInt16 addr16)
		{
			_addr16 = addr16;
		}

		public void setBroadcastRadius(byte broadcastRadius)
		{
			_broadcastRadius = broadcastRadius;
		}

		public void setOption(byte option)
		{
			_option = option;
		}

		// declare virtual functions
		public override byte getFrameData(byte pos)
		{
			if (pos == 0)
			{
				return (byte)((_addr64.getMsb() >> 24) & 0xff);
			}
			else if (pos == 1)
			{
				return (byte)((_addr64.getMsb() >> 16) & 0xff);
			}
			else if (pos == 2)
			{
				return (byte)((_addr64.getMsb() >> 8) & 0xff);
			}
			else if (pos == 3)
			{
				return (byte)(_addr64.getMsb() & 0xff);
			}
			else if (pos == 4)
			{
				return (byte)((_addr64.getLsb() >> 24) & 0xff);
			}
			else if (pos == 5)
			{
				return (byte)((_addr64.getLsb() >> 16) & 0xff);
			}
			else if (pos == 6)
			{
				return (byte)((_addr64.getLsb() >> 8) & 0xff);
			}
			else if (pos == 7)
			{
				return (byte)(_addr64.getLsb() & 0xff);
			}
			else if (pos == 8)
			{
				return (byte)((_addr16 >> 8) & 0xff);
			}
			else if (pos == 9)
			{
				return (byte)(_addr16 & 0xff);
			}
			else if (pos == 10)
			{
				return _broadcastRadius;
			}
			else if (pos == 11)
			{
				return _option;
			}
			else
			{
				return getPayload()[pos - XBee.ZB_TX_API_LENGTH];
			}
		}

		public override byte getFrameDataLength()
		{
			return (byte)(XBee.ZB_TX_API_LENGTH + getPayloadLength());
		}
		protected XBeeAddress64 _addr64;
		protected UInt16 _addr16;
		protected byte _broadcastRadius;
		protected byte _option;
	};

	/**
	 * Represents a Series 2 TX packet that corresponds to Api Id: ZB_EXPLICIT_TX_REQUEST
	 *
	 * See the warning about maximum packet size for ZBTxRequest above,
	 * which probably also applies here as well.
	 *
	 * Note that to distinguish reply packets from non-XBee devices, set
	 * AO=1 to enable reception of ZBExplicitRxResponse packets.
	 */
	public class ZBExplicitTxRequest : ZBTxRequest 
	{
		/**
		 * Creates a unicast ZBExplicitTxRequest with the ACK option and
		 * DEFAULT_FRAME_ID.
		 *
		 * It uses the Maxstream profile (0xc105), both endpoints 232
		 * and cluster 0x0011, resulting in the same packet as sent by a
		 * normal ZBTxRequest.
		 */
		public ZBExplicitTxRequest(XBeeAddress64 addr64, byte [] payload, byte payloadLength) : base(addr64, payload, payloadLength)
		{
			_srcEndpoint = XBee.DEFAULT_ENDPOINT;
			_dstEndpoint = XBee.DEFAULT_ENDPOINT;
			_profileId = XBee.DEFAULT_PROFILE_ID;
			_clusterId = XBee.DEFAULT_CLUSTER_ID;
			setApiId(XBee.ZB_EXPLICIT_TX_REQUEST);
		}

		/**
		 * Create a ZBExplicitTxRequest, specifying all fields.
		 */
		public ZBExplicitTxRequest(XBeeAddress64 addr64, UInt16 addr16, byte broadcastRadius, byte option, byte [] payload, byte payloadLength, byte frameId, byte srcEndpoint, byte dstEndpoint, UInt16 clusterId, UInt16 profileId)
						: base(addr64, addr16, broadcastRadius, option, payload, payloadLength, frameId)
		{
			_srcEndpoint = srcEndpoint;
			_dstEndpoint = dstEndpoint;
			_profileId = profileId;
			_clusterId = clusterId;
			setApiId(XBee.ZB_EXPLICIT_TX_REQUEST);
		}

		/**
		 * Creates a default instance of this class.  At a minimum you
		 * must specify a payload, payload length and a destination
		 * address before sending this request.
		 *
		 * Furthermore, it uses the Maxstream profile (0xc105), both
		 * endpoints 232 and cluster 0x0011, resulting in the same
		 * packet as sent by a normal ZBExplicitTxRequest.
		 */
		public ZBExplicitTxRequest() : base()
		{
			_srcEndpoint = XBee.DEFAULT_ENDPOINT;
			_dstEndpoint = XBee.DEFAULT_ENDPOINT;
			_profileId = XBee.DEFAULT_PROFILE_ID;
			_clusterId = XBee.DEFAULT_CLUSTER_ID;
			setApiId(XBee.ZB_EXPLICIT_TX_REQUEST);
		}

		public byte getSrcEndpoint()
		{
			return _srcEndpoint;
		}

		public byte getDstEndpoint()
		{
			return _dstEndpoint;
		}

		public UInt16 getClusterId()
		{
			return _clusterId;
		}

		public UInt16 getProfileId()
		{
			return _profileId;
		}

		public void setSrcEndpoint(byte endpoint)
		{
			_srcEndpoint = endpoint;
		}

		public void setDstEndpoint(byte endpoint)
		{
			_dstEndpoint = endpoint;
		}

		public void setClusterId(UInt16 clusterId)
		{
			_clusterId = clusterId;
		}

		public void setProfileId(UInt16 profileId)
		{
			_profileId = profileId;
		}

		// declare virtual functions
		public override byte getFrameDataLength()
		{
			return (byte)(XBee.ZB_EXPLICIT_TX_API_LENGTH + getPayloadLength());
		}

		public override byte getFrameData(byte pos)
		{
			if (pos < 10)
			{
				return base.getFrameData(pos);
			}
			else if (pos == 10)
			{
				return _srcEndpoint;
			}
			else if (pos == 11)
			{
				return _dstEndpoint;
			}
			else if (pos == 12)
			{
				return (byte)((_clusterId >> 8) & 0xff);
			}
			else if (pos == 13)
			{
				return (byte)(_clusterId & 0xff);
			}
			else if (pos == 14)
			{
				return (byte)((_profileId >> 8) & 0xff);
			}
			else if (pos == 15)
			{
				return (byte)(_profileId & 0xff);
			}
			else if (pos == 16)
			{
				return _broadcastRadius;
			}
			else if (pos == 17)
			{
				return _option;
			}
			else
			{
				return getPayload()[pos - XBee.ZB_EXPLICIT_TX_API_LENGTH];
			}
		}

		private byte _srcEndpoint;
		private byte _dstEndpoint;
		private UInt16 _profileId;
		private UInt16 _clusterId;
	}

#endif

	/**
	 * Represents an AT Command TX packet
	 * The command is used to configure the serially connected XBee radio
	 */
	public abstract class AtCommandRequest : XBeeRequest 
	{
		public AtCommandRequest() : base(XBee.AT_COMMAND_REQUEST, XBee.DEFAULT_FRAME_ID)
		{
			_command = null;
			clearCommandValue();
		}

		public AtCommandRequest(byte [] command) : base(XBee.AT_COMMAND_REQUEST, XBee.DEFAULT_FRAME_ID)
		{
			_command = command;
			clearCommandValue();
		}

		public AtCommandRequest(byte [] command, byte [] commandValue, byte commandValueLength) : base(XBee.AT_COMMAND_REQUEST, XBee.DEFAULT_FRAME_ID)
		{
			_command = command;
			_commandValue = commandValue;
			_commandValueLength = commandValueLength;
		}

		public override byte getFrameData(byte pos)
		{

			if (pos == 0)
			{
				return _command[0];
			}
			else if (pos == 1)
			{
				return _command[1];
			}
			else
			{
				return _commandValue[pos - XBee.AT_COMMAND_API_LENGTH];
			}
		}

		public override byte getFrameDataLength()
		{
			// command is 2 byte + length of value
			return (byte)(XBee.AT_COMMAND_API_LENGTH + _commandValueLength);
		}

		public byte [] getCommand()
		{
			return _command;
		}

		public void setCommand(byte [] command)
		{
			_command = command;
		}

		public byte[] getCommandValue()
		{
			return _commandValue;
		}

		public void setCommandValue(byte [] value)
		{
			_commandValue = value;
		}

		public byte getCommandValueLength()
		{
			return _commandValueLength;
		}

		public void setCommandValueLength(byte length)
		{
			_commandValueLength = length;
		}

		/**
		 * Clears the optional commandValue and commandValueLength so that a query may be sent
		 */
		public void clearCommandValue()
		{
			_commandValue = null;
			_commandValueLength = 0;
		}

		//void reset();
		
		private byte [] _command;
		private byte [] _commandValue;
		private byte  _commandValueLength;
	}

	/**
	 * Represents an Remote AT Command TX packet
	 * The command is used to configure a remote XBee radio
	 */
	public class RemoteAtCommandRequest : AtCommandRequest 
	{
		public RemoteAtCommandRequest() : base(null, null, 0)
		{
			_remoteAddress16 = 0;
			_applyChanges = false;
			setApiId(XBee.REMOTE_AT_REQUEST);
		}

		/**
		 * Creates a RemoteAtCommandRequest with 16-bit address to set a command.
		 * 64-bit address defaults to broadcast and applyChanges is true.
		 */
		public RemoteAtCommandRequest(UInt16 remoteAddress16, byte [] command, byte[] commandValue, byte commandValueLength) : base(command, commandValue, commandValueLength)
		{
			_remoteAddress64 = broadcastAddress64;
			_remoteAddress16 = remoteAddress16;
			_applyChanges = true;
			setApiId(XBee.REMOTE_AT_REQUEST);
		}

		/**
		 * Creates a RemoteAtCommandRequest with 16-bit address to query a command.
		 * 64-bit address defaults to broadcast and applyChanges is true.
		 */
		public RemoteAtCommandRequest(UInt16 remoteAddress16, byte [] command) : base(command, null, 0)
		{
			_remoteAddress64 = broadcastAddress64;
			_remoteAddress16 = remoteAddress16;
			_applyChanges = false;
			setApiId(XBee.REMOTE_AT_REQUEST);
		}

		/**
		 * Creates a RemoteAtCommandRequest with 64-bit address to set a command.
		 * 16-bit address defaults to broadcast and applyChanges is true.
		 */
		public RemoteAtCommandRequest(XBeeAddress64 remoteAddress64, byte [] command, byte [] commandValue, byte commandValueLength) : base(command, commandValue, commandValueLength)
		{
			_remoteAddress64 = remoteAddress64;
			// don't worry.. works for series 1 too!
			_remoteAddress16 = XBee.ZB_BROADCAST_ADDRESS;
			_applyChanges = true;
			setApiId(XBee.REMOTE_AT_REQUEST);
		}

		/**
		 * Creates a RemoteAtCommandRequest with 16-bit address to query a command.
		 * 16-bit address defaults to broadcast and applyChanges is true.
		 */
		public RemoteAtCommandRequest(XBeeAddress64 remoteAddress64, byte [] command) : base(command, null, 0)
		{
			_remoteAddress64 = remoteAddress64;
			_remoteAddress16 = XBee.ZB_BROADCAST_ADDRESS;
			_applyChanges = false;
			setApiId(XBee.REMOTE_AT_REQUEST);
		}

		public UInt16 getRemoteAddress16()
		{
			return _remoteAddress16;
		}

		public void setRemoteAddress16(UInt16 remoteAddress16)
		{
			_remoteAddress16 = remoteAddress16;
		}

		public XBeeAddress64 getRemoteAddress64() 
		{
			return _remoteAddress64;
		}

		public void setRemoteAddress64(XBeeAddress64 remoteAddress64)
		{
			_remoteAddress64 = remoteAddress64;
		}

		public bool getApplyChanges()
		{
			return _applyChanges;
		}

		public void setApplyChanges(bool applyChanges)
		{
			_applyChanges = applyChanges;
		}

		public override byte getFrameData(byte pos)
		{
			if (pos == 0)
			{
				return (byte)((_remoteAddress64.getMsb() >> 24) & 0xff);
			}
			else if (pos == 1)
			{
				return (byte)((_remoteAddress64.getMsb() >> 16) & 0xff);
			}
			else if (pos == 2)
			{
				return (byte)((_remoteAddress64.getMsb() >> 8) & 0xff);
			}
			else if (pos == 3)
			{
				return (byte)(_remoteAddress64.getMsb() & 0xff);
			}
			else if (pos == 4)
			{
				return (byte)((_remoteAddress64.getLsb() >> 24) & 0xff);
			}
			else if (pos == 5)
			{
				return (byte)((_remoteAddress64.getLsb() >> 16) & 0xff);
			}
			else if (pos == 6)
			{
				return (byte)((_remoteAddress64.getLsb() >> 8) & 0xff);
			}
			else if (pos == 7)
			{
				return (byte)(_remoteAddress64.getLsb() & 0xff);
			}
			else if (pos == 8)
			{
				return (byte)((_remoteAddress16 >> 8) & 0xff);
			}
			else if (pos == 9)
			{
				return (byte)(_remoteAddress16 & 0xff);
			}
			else if (pos == 10)
			{
				return (byte)(_applyChanges ? 2 : 0);
			}
			else if (pos == 11)
			{
				return getCommand()[0];
			}
			else if (pos == 12)
			{
				return getCommand()[1];
			}
			else
			{
				return getCommandValue()[pos - XBee.REMOTE_AT_COMMAND_API_LENGTH];
			}
		}

		public override byte getFrameDataLength()
		{
			return (byte)(XBee.REMOTE_AT_COMMAND_API_LENGTH + getCommandValueLength());
		}
		public static XBeeAddress64 broadcastAddress64 = new XBeeAddress64(0x0, XBee.BROADCAST_ADDRESS);
		//	static uint16_t broadcast16Address;
		private XBeeAddress64 _remoteAddress64;
		private UInt16 _remoteAddress16;
		private bool _applyChanges;
	}
}
