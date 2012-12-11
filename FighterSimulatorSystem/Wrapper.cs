using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace FighterSimulatorSystem
{
    public static class Wrapper
    {

        // typedef unsigned int uint32_t;
        // typedef unsigned short uint16_t;
        // typedef unsigned char uint8_t;

        public enum PIXEL_FORMAT
        {
        	PF_1BPP = 0,
        	PF_1BPP_DS,
        	PF_2BPP,
        	PF_4BPP,
        	PF_8BPP,
        	PF_16BPP_RGB565,
        	PF_16BPP_BGR565,
        	PF_32BPP
        } ;

        public  struct DisplayInfo
        {
            UInt32 Width;
            UInt32 Height;
            UInt32 PixelFormat;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)] 
            char[] Name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=16)] 
            char[] SerialNumber;
        	UInt32 Handle;
        	UInt32 Version;
        } ;

        //*****************************************************************************
        //
        //! Returns the number of attached displays.
        //!
        //! This function returns the number of attached displays. Returns the
        //! number of all attached displays even if they are in use and not 
        //! available.
        //!
        //! \return Returns the number of displays attached.
        //
        //*****************************************************************************
        [DllImport("USBD480_lib.dll")]
        public static extern int USBD480_GetNumberOfDisplays();

        //*****************************************************************************
        //
        //! Get display configuration information for specified display.
        //!
        //! \param index is the number of the display for which the information is read.
        //! \param di is a pointer to the \e DisplayInfo structure where the information 
        //! will be saved.
        //!
        //! This function fills the configuration data structure for the display which 
        //! index is given in parameter \e index.
        //! The index number starts from 0. If you have two displays connected they 
        //! have index numbers 0 and 1.
        //!
        //! \return Returns USBD480_OK if configuration was read successfully. In
        //! case of a failure USBD480_ERROR is returned. If the display is not 
        //! available USBD480_ERROR is returned.
        //
        //*****************************************************************************
        [DllImport("USBD480_lib.dll")]
        public static extern int USBD480_GetDisplayConfiguration(UInt32 index, ref DisplayInfo di);

        //*****************************************************************************
        //
        //! Open the display.
        //!
        //! \param di is a pointer to the \e DisplayInfo structure of the display 
        //! to open.
        //! \param flags should be 0.
        //!
        //! This function opens the display and prepares it for further use. 
        //! When working with multiple displays the \e DisplayInfo structure 
        //! for each display is first filled with USBD480_GetDisplayConfiguration() 
        //! and then passed to USBD480_Open().
        //!
        //! \return Returns USBD480_OK if display was opened successfully. In
        //! case of a failure USBD480_ERROR is returned.
        //
        //*****************************************************************************
        [DllImport("USBD480_lib.dll")]
        public static extern int USBD480_Open(ref DisplayInfo di, UInt32 flags);

        //*****************************************************************************
        //
        //! Close the display.
        //!
        //! \param di is a pointer to the \e DisplayInfo structure of the display 
        //! to close.
        //!
        //! This function closes the display and frees any allocated resources. 
        //!
        //! \return Returns USBD480_OK if display was closed successfully. If
        //! supplied DisplayInfo structure is invalid USBD480_ERROR is returned.
        //
        //*****************************************************************************
        [DllImport("USBD480_lib.dll")]
        public static extern int USBD480_Close(ref DisplayInfo di);

        
        //*****************************************************************************
        //
        //! Draw full screen image to the display from RGBA buffer.
        //!
        //! \param di is a pointer to the \e DisplayInfo structure of the display 
        //! to use.
        //! \param fb is a pointer to the buffer including the data to write to the 
        //! display.
        //!
        //! This function updates the full screen with data from the buffer. The buffer
        //! size needs to be at least display width * display heigth * 4 bytes in size 
        //! (for RGB pixel data). Format is RGBA => 0xAABBGGRR
        //!
        //! \return Returns USBD480_OK if success. In
        //! case of a failure USBD480_ERROR is returned.
        //
        //*****************************************************************************
        [DllImport("USBD480_lib.dll")]
        public static extern int  USBD480_DrawFullScreenRGBA32(ref DisplayInfo di, UInt32[] fb);

        //*****************************************************************************
        //
        //! Set display brightness.
        //!
        //! \param di is a pointer to the \e DisplayInfo structure of the display 
        //! to use.
        //! \param brightness is the backlight brightness. (0-255)
        //!
        //! This function sets the backlight values.
        //!
        //! \return Returns USBD480_OK if success. In
        //! case of a failure USBD480_ERROR is returned.
        //
        //*****************************************************************************
        [DllImport("USBD480_lib.dll")]
        public static extern int USBD480_SetBrightness(ref DisplayInfo di, UInt32 brightness);


        // ================= LCD2USB =============================================
        //------

        // Exported DLL fucntions

        // Name
        [DllImport("LCD2USB.dll")]
        public static extern string DISPLAYDLL_DriverName();

        // Usage
        [DllImport("LCD2USB.dll")]
        public static extern string  DISPLAYDLL_Usage();

        // Init
        //[DllImport("LCD2USB.dll")]
        //public static extern string  DISPLAYDLL_Init(byte x_size,  byte y_size, char[] startparam, out bool ok);
        
        /// Return Type: char*
        ///x_size: BYTE->unsigned char
        ///y_size: BYTE->unsigned char
        ///startparam: char*
        ///ok: boolean*
        [DllImport("LCD2USB.dll")]
        public static extern string DISPLAYDLL_Init(byte x_size, byte y_size, [System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string startparam, ref bool ok);

        // Fini
        [DllImport("LCD2USB.dll")]
        public static extern  void DISPLAYDLL_Done();

        [DllImport("LCD2USB.dll")]
        public static extern  void DISPLAYDLL_SetBacklight(bool on);

        // Contrast
        [DllImport("LCD2USB.dll")]
        public static extern  void DISPLAYDLL_SetContrast( byte val);
        
        // Brightness
        [DllImport("LCD2USB.dll")]
        public static extern  void DISPLAYDLL_SetBrightness( byte val);

        // write a string
        [DllImport("LCD2USB.dll")]
        public static extern void DISPLAYDLL_Write([System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string str);

        // set cursor position
        [DllImport("LCD2USB.dll")]
        public static extern  void DISPLAYDLL_SetPosition(byte x,  byte y);

        // define custom character
        [DllImport("LCD2USB.dll")]
        public static extern  void DISPLAYDLL_CustomChar( byte ascii, ref char[] bitmap);
    }
}
