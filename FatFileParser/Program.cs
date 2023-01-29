﻿using System.Text;

var files = new Dictionary<string, Tuple<byte[], byte[]>>();

using (var fatFile = File.OpenRead("DATA.FAT"))
{
    // scan past file header
    fatFile.Read(new byte[8], 0, 8);

    var fileEntry = new byte[28];

    while (fatFile.Read(fileEntry, 0, fileEntry.Length) > 0)
    {
        var fileName = Encoding.ASCII.GetString(fileEntry[..12]).Replace("\0", string.Empty);

        if (files.ContainsKey(fileName))
        {
            fileName = fileName.Replace(".", "-1.");
        }

        files.Add(
            fileName,
            new Tuple<byte[], byte[]>(
                fileEntry[16..20],
                fileEntry[20..]
            )
        );
    }
}


using (var listingFile = new StreamWriter("file-listing.txt"))
{
    foreach (var (file, dataBytes) in files)
    {
        var offset = BitConverter.ToUInt32(dataBytes.Item1);
        var length = BitConverter.ToUInt32(dataBytes.Item2);
 
        listingFile.WriteLine($"{file},{offset},{length}");
    }
}

Console.WriteLine("DONE");