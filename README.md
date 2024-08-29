# Information
Apple Mobile Device Service fails to connect to an iphone wirelessly on a network after the iphone has previously disconnected from the network. This service is needed to register a connection between the phone and computer wirelessly. The only options are to either restart the 
service or to restart the computer and plug in the phone to pair it again. This service will run an HTTP client service in the background of a windows machine that accepts a GET request from an iphone IOS shortcut with an authentication token on a specified port. This GET request 
is processed if the authetication token sent from the phone matches what is stored in the service config.json file. The GET request from the phone will trigger an event to restart the Apple Mobile Device Service. This will allow the phone to wirelessly conenct to the computer 
and allow the phone to sign sideloaded apps wirelessly.


# Install instructions
1. Download the ZIP
2. Extract the ZIP to your C:\ drive in the root of the drive (C:/AltStoreAutomation)
3. Open Command Prompt in Administrator
4. Type: "cd C:\AltStoreAutomation"
5. Go to the following folder: "C:\Windows\Microsoft.NET\Framework64"
6. Find the folder starting with v4.##
7. Copy this directory "C:\Windows\Microsoft.NET\Framework64\v4.#.####\InstallUtil.exe" (Insert your version numebrs)
8. Go back to command prompt and paste the full path "C:\Windows\Microsoft.NET\Framework64\v4.#.####\InstallUtil.exe /i AltStoreAutomation.exe"
9. Open Task Manager, you should now see AltStoreAutomation under Services Tab

# After Install
1. Open the config.json file in the AltStoreAutomation folder. You will have to change the port to whatever port you are going to forward for this. 
2. Set your authentication token to whatever you want it to be, this will be used in the IOS shortcut
3. Save the file
4. Start the AltStoreAutomation service 

# IOS Shortcut
1. Open Shortcuts
2. Search Wi-Fi
3. When => Choose Network you want to connect
4. Select Run Immediately
5. Next => New Blank Automation
6. Add Action => Search for "Get Cotents of URL"
7. Under the URL type http://<your host ip>:<yourport>/<yourauthtoken>
8. Expand the side and you'll see Method and Headers. Leave Method as GET
9. Add a new Header => Key = "Authentication" => Text = "Important <YOURAUTHTOKEN>"
10. Search Actions => Get Text from Input => Make the input your url you passed to the host, make this response a variable
11. Create an if statement
12. If Text contains "success" then add a show notification
13. Under otherwise => show notification for failure and add in the text from the response
14. Click Done

