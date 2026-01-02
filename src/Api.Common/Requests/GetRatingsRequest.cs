using Api.Common.Responses;
using Mediator;

namespace Api.Common.Requests;

public sealed class GetRatingsRequest : BaseRequest, IRequest<GetRatingsResponse>;