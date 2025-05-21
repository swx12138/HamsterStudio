using HamsterStudio.RedBook.DataModels;

namespace HamsterStudio.RedBook.Interfaces;

public interface IRedBookParser
{
    NoteDataModel? GetNoteData(string url);
}
