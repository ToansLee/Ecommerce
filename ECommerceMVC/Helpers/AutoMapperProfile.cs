using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Models;
using ECommerceMVC.ViewModels;

namespace ECommerceMVC.Helpers
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
		CreateMap<RegisterVM, Customer>()
			.ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.MaKh))
			.ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.HoTen))
			.ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.GioiTinh))
			.ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.NgaySinh))
			.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.DiaChi))
			.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? ""))
			.ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.DienThoai))
			.ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.MatKhau))
			.ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));
		}
	}
}
