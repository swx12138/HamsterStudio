using HamsterStudio.RedBook.Models;

namespace HamsterStudio.RedBook.Interfaces;

public interface IRedBookParser
{
    NoteDataModel? GetNoteData(string url);
}
