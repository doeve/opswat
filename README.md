# opswat

we have 2 central fies: Program.cs, and ApiClient.cs. The Program.cs is the entry, and tha ApiClient is a class containing the methods to manage the retrieval/uploading of data. Using a command line argument, we can specify the file we want to process.

The ApiClient has the following methods:
    - GetHash(filePath): calculates the SHA256 hash of the file
    - HashLookup(hash): accesses the metadefender API in order to look for the given SHA256 hash
    - Upload(): uploads the given file if its hash has not been found, and gets us a data_id
    - getResult(): checks if the file has been verified
    - DisplayInfo(): displays the scan results in function of each engine

Example call with unused file:

scanner.exe "file.jpg"
