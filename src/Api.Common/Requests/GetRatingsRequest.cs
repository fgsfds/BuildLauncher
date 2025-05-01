using Api.Common.Responses;
using MediatR;

namespace Api.Common.Requests;

public sealed class GetRatingsRequest : BaseRequest, IRequest<GetRatingsResponse>;