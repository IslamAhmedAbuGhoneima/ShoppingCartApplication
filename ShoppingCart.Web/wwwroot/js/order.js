let dtble;

$(document).ready(function () {
    loadData();
});





function loadData() {
    dtble = $("#orders-table").DataTable({
        "ajax": {
            "url": "/Admin/Order/GetData"
        },
        "columns": [
            { "data": "id" },
            { "data": "userName" },
            { "data": "phoneNumber" },
            { "data": "email" },
            { "data": "orderStatus" },
            { "data": "totalPrice" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <a href="/Admin/Order/Details/${data}" class="btn btn-warning">Details</a>

                    `
                }
            }
        ]
    });
}


const orderStatusElement = document.querySelector(".order-status");

async function startProccess(id) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    const data = await fetch(`/Admin/Order/StartProccess/${id}`,
        {
            method: "POST",
            headers: {
                "Content-Type": "Application/json",
                "RequestVerificationToken": token
            }
        });
    const response = await data.json();

    if (response.success)
    {
        orderStatusElement.innerHTML = response.orderStatus;
        Swal.fire({
            position: "center",
            icon: "success",
            title: "Order Started Proccessing",
            showConfirmButton: false,
            timer: 1500
        });
        location.reload();
    }
    else
    {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Something went wrong!",
        });
    }

}


async function startShipping(id) {

    const orderCarrierElement = document.getElementById("Carrier");
    const orderTrackingNumberElement = document.getElementById("TrackingNumber");
    const shippingDateElement = document.getElementById("ShippingDate");

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;


    if (orderCarrierElement.value == '' || orderTrackingNumberElement.value == '') {
        Swal.fire({
            icon: "error",
            title: "Carrier and TrackingNumber shouldn't be empty :( ",
            text: "Something went wrong!",
        });
    } else {
        const data = await fetch(`/Admin/Order/StartShipping/${id}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token
            },
            body: JSON.stringify({ orderCarrier: orderCarrierElement.value, orderTrackingNumber: orderTrackingNumberElement.value })
        });
        const response = await data.json();

        if (response.success)
        {
            orderStatusElement.innerHTML = response.orderStatus;
            orderCarrierElement.value = response.carrier;
            orderTrackingNumberElement.value = response.trackingNumber
            shippingDateElement.value = response.shippingDate;
            Swal.fire({
                icon: "success",
                title: "Order Started Shipping",
                showConfirmButton: false,
                timer: 1500
            });
            location.reload();
        }
        else
        {
            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "Something went wrong!",
            });
        }
    }
}


const cancelOrder = async (id) => {
    const token = document.querySelector("input[name='__RequestVerificationToken']").value;
    const data = await fetch(`/Admin/Order/CancelOrder/${id}`,
        {
            method: "POST",
            headers: {
                "Content-Type": "Application/json",
                "RequestVerificationToken": token
            }
        });

    const response = await data.json();
    if (response.success) {
        location.reload();
    } else {
        Swal.fire({
            icon: "error",
            title: "Oops...",
            text: "Something went wrong!",
        });
    }

}