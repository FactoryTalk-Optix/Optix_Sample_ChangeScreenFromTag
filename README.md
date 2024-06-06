# ChangeScreenFromTag

This example shows how to change the displayed page based on a value coming from the PLC or a Model variable and send back the current page (if changed from the HMI)

## Usage

1. Create as many screens as needed into the `Screens` folder (or any `Screens` subfolder)
1. Make sure each screen that should be indexed has a `ScreenId` variable with Int32 as DataType
1. Make sure the ScreenId variable has unique values across the whole screens (any value less or equal to zero is ignored)
1. Execute project

## Testing

1. Change the value of the tag using the `SpinBox`, if value is bigger than zero and a page with that ID exists, the `PanelLoader` is refreshed
1. Change the page using the buttons to interact with the `PanelLoader`, the `ScreenId` for the current panel is sent to the controller

## Important note

- This procedure works if only a single session (either a NativePresentationEngine or one session of the WebPresentationEngine) is used, because the PLC Tag is shared across all sessions

### Disclaimer

Rockwell Automation maintains these repositories as a convenience to you and other users. Although Rockwell Automation reserves the right at any time and for any reason to refuse access to edit or remove content from this Repository, you acknowledge and agree to accept sole responsibility and liability for any Repository content posted, transmitted, downloaded, or used by you. Rockwell Automation has no obligation to monitor or update Repository content

The examples provided are to be used as a reference for building your own application and should not be used in production as-is. It is recommended to adapt the example for the purpose, observing the highest safety standards.
